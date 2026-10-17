using System.Runtime.CompilerServices;

using Toolbox.Tools;

namespace Toolbox.Test;

public class AsyncEnumerableExtensionsTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Theory]
    [InlineData(1, null)]
    [InlineData(2, 1)]
    [InlineData(4, 3)]
    public async Task ValidArguments_ProduceEveryResultExactlyOnce(int parallelism, int? capacity)
    {
        List<int> results = await DrainAsync(RangeAsync(100).SelectParallelAsync(
            (item, _) => ValueTask.FromResult(item * 2), parallelism, capacity)).WaitAsync(Timeout);

        results.Count.Be(100);
        results.Order().SequenceEqual(Enumerable.Range(0, 100).Select(x => x * 2)).BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidParallelism_IsRejected(int parallelism)
    {
        await Verify.ThrowsAsync<ArgumentException>(() => DrainAsync(
            RangeAsync(1).SelectParallelAsync((item, _) => ValueTask.FromResult(item), parallelism)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidCapacity_IsRejected(int capacity)
    {
        await Verify.ThrowsAsync<ArgumentException>(() => DrainAsync(
            RangeAsync(1).SelectParallelAsync((item, _) => ValueTask.FromResult(item), 1, capacity)));
    }

    [Theory]
    [InlineData(1073741824)]
    [InlineData(int.MaxValue)]
    public async Task DefaultCapacityOverflow_IsRejectedBeforeStartingSource(int parallelism)
    {
        bool started = false;
        await Verify.ThrowsAsync<ArgumentException>(() => DrainAsync(
            Source().SelectParallelAsync((item, _) => ValueTask.FromResult(item), parallelism)));
        started.BeFalse();

        async IAsyncEnumerable<int> Source()
        {
            started = true;
            await Task.CompletedTask;
            yield return 1;
        }
    }

    [Fact]
    public async Task NullArguments_AreRejected()
    {
        await Verify.ThrowsAsync<ArgumentNullException>(() => DrainAsync(
            AsyncEnumerableExtensions.SelectParallelAsync<int, int>(null!,
                (item, _) => ValueTask.FromResult(item), 1)));
        await Verify.ThrowsAsync<ArgumentNullException>(() => DrainAsync(
            RangeAsync(1).SelectParallelAsync<int, int>(null!, 1)));
    }

    [Fact]
    public async Task EmptySource_DoesNotInvokeSelector()
    {
        int calls = 0;
        List<int> results = await DrainAsync(RangeAsync(0).SelectParallelAsync((item, _) =>
        {
            Interlocked.Increment(ref calls);
            return ValueTask.FromResult(item);
        }, 3)).WaitAsync(Timeout);

        results.Count.Be(0);
        calls.Be(0);
    }

    [Fact]
    public async Task Results_AreUnorderedAndStreamBeforeSourceCompletes()
    {
        var releaseFirst = NewSignal();
        var secondStarted = NewSignal();
        var releaseSource = NewSignal();
        using var safety = new CancellationTokenSource(Timeout);
        await using var iterator = Source().SelectParallelAsync(SelectAsync, 2, 1, safety.Token)
            .GetAsyncEnumerator();

        try
        {
            (await iterator.MoveNextAsync().AsTask().WaitAsync(Timeout)).BeTrue();
            iterator.Current.Be(1);
            await secondStarted.Task.WaitAsync(Timeout);
            releaseFirst.SetResult();
            (await iterator.MoveNextAsync().AsTask().WaitAsync(Timeout)).BeTrue();
            iterator.Current.Be(0);
            releaseSource.SetResult();
            (await iterator.MoveNextAsync().AsTask().WaitAsync(Timeout)).BeFalse();
        }
        finally
        {
            releaseFirst.TrySetResult();
            releaseSource.TrySetResult();
        }

        async IAsyncEnumerable<int> Source([EnumeratorCancellation] CancellationToken token = default)
        {
            yield return 0;
            yield return 1;
            await releaseSource.Task.WaitAsync(token);
        }

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            if (item == 0) await releaseFirst.Task.WaitAsync(token);
            else secondStarted.TrySetResult();
            return item;
        }
    }

    [Fact]
    public async Task SelectorConcurrency_IsCappedAtRequestedParallelism()
    {
        const int parallelism = 3;
        int active = 0;
        int peak = 0;
        var allStarted = NewSignal();
        var release = NewSignal();
        using var safety = new CancellationTokenSource(Timeout);
        Task<List<int>> run = DrainAsync(RangeAsync(30).SelectParallelAsync(SelectAsync,
            parallelism, 1, safety.Token));

        try
        {
            await allStarted.Task.WaitAsync(Timeout);
            Volatile.Read(ref active).Be(parallelism);
        }
        finally
        {
            release.TrySetResult();
        }

        (await run.WaitAsync(Timeout)).Count.Be(30);
        peak.Be(parallelism);
        active.Be(0);

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            int current = Interlocked.Increment(ref active);
            UpdatePeak(current);
            if (current == parallelism) allStarted.TrySetResult();
            try
            {
                await release.Task.WaitAsync(token);
                return item;
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        }

        void UpdatePeak(int value)
        {
            int previous;
            do
            {
                previous = Volatile.Read(ref peak);
                if (previous >= value) return;
            } while (Interlocked.CompareExchange(ref peak, value, previous) != previous);
        }
    }

    [Fact]
    public async Task SlowConsumer_BoundsBothBuffers_AndDisposalStopsProducer()
    {
        int produced = 0;
        bool sourceDisposed = false;
        var saturated = NewSignal();
        using var safety = new CancellationTokenSource(Timeout);
        await using var iterator = Source().SelectParallelAsync(
            (item, _) => ValueTask.FromResult(item), 2, 1, safety.Token).GetAsyncEnumerator();

        (await iterator.MoveNextAsync().AsTask().WaitAsync(Timeout)).BeTrue();
        await saturated.Task.WaitAsync(Timeout);
        // One delivered result + one output + two workers + one input + one pending source item.
        Volatile.Read(ref produced).Be(6);
        await iterator.DisposeAsync().AsTask().WaitAsync(Timeout);
        sourceDisposed.BeTrue();
        produced.Be(6);

        async IAsyncEnumerable<int> Source([EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.CompletedTask;
            try
            {
                for (int item = 0; ; item++)
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Increment(ref produced) == 6) saturated.TrySetResult();
                    yield return item;
                }
            }
            finally
            {
                sourceDisposed = true;
            }
        }
    }

    [Fact]
    public async Task WorkerFailure_CancelsBlockedProducer_AndJoinsSibling()
    {
        var expected = new InvalidOperationException("selector failed");
        var producerBlocked = NewSignal();
        var siblingStarted = NewSignal();
        var siblingStopping = NewSignal();
        var releaseCleanup = NewSignal();
        bool sourceDisposed = false;
        bool siblingStopped = false;
        using var safety = new CancellationTokenSource(Timeout);
        Task<List<int>> run = DrainAsync(Source().SelectParallelAsync(SelectAsync, 2, 1, safety.Token));

        try
        {
            await siblingStopping.Task.WaitAsync(Timeout);
            run.IsCompleted.BeFalse();
        }
        finally
        {
            releaseCleanup.TrySetResult();
        }

        await VerifyFailureAsync(run, expected);
        sourceDisposed.BeTrue();
        siblingStopped.BeTrue();

        async IAsyncEnumerable<int> Source([EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.CompletedTask;
            try
            {
                for (int item = 0; ; item++)
                {
                    token.ThrowIfCancellationRequested();
                    if (item == 3) producerBlocked.TrySetResult();
                    yield return item;
                }
            }
            finally
            {
                sourceDisposed = true;
            }
        }

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            if (item == 0)
            {
                await siblingStarted.Task.WaitAsync(token);
                await producerBlocked.Task.WaitAsync(token);
                throw expected;
            }

            siblingStarted.TrySetResult();
            try
            {
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
                return item;
            }
            finally
            {
                siblingStopping.TrySetResult();
                await releaseCleanup.Task;
                siblingStopped = true;
            }
        }
    }

    [Fact]
    public async Task SourceFailure_PreservesOriginalException_AndJoinsAllWorkers()
    {
        var expected = new InvalidOperationException("source failed");
        var workersStarted = NewSignal();
        var workersStopping = NewSignal();
        var releaseCleanup = NewSignal();
        int started = 0;
        int stopping = 0;
        int stopped = 0;
        using var safety = new CancellationTokenSource(Timeout);
        Task<List<int>> run = DrainAsync(Source().SelectParallelAsync(SelectAsync, 2, 1, safety.Token));

        try
        {
            await workersStopping.Task.WaitAsync(Timeout);
            run.IsCompleted.BeFalse();
        }
        finally
        {
            releaseCleanup.TrySetResult();
        }

        await VerifyFailureAsync(run, expected);
        stopped.Be(2);

        async IAsyncEnumerable<int> Source([EnumeratorCancellation] CancellationToken token = default)
        {
            yield return 0;
            yield return 1;
            await workersStarted.Task.WaitAsync(token);
            throw expected;
        }

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            if (Interlocked.Increment(ref started) == 2) workersStarted.TrySetResult();
            try
            {
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
                return item;
            }
            finally
            {
                if (Interlocked.Increment(ref stopping) == 2) workersStopping.TrySetResult();
                await releaseCleanup.Task;
                Interlocked.Increment(ref stopped);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancellation_StopsSourceAndWorkers(bool enumeratorToken)
    {
        var started = NewSignal();
        bool sourceDisposed = false;
        bool workerStopped = false;
        using var cancellation = new CancellationTokenSource(Timeout);
        IAsyncEnumerable<int> query = Source().SelectParallelAsync(SelectAsync, 1, 1,
            enumeratorToken ? default : cancellation.Token);
        Task<List<int>> run = DrainAsync(query, enumeratorToken ? cancellation.Token : default);

        await started.Task.WaitAsync(Timeout);
        await cancellation.CancelAsync();
        await Verify.ThrowsAsync<OperationCanceledException>(async () => await run.WaitAsync(Timeout));
        sourceDisposed.BeTrue();
        workerStopped.BeTrue();

        async IAsyncEnumerable<int> Source([EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.CompletedTask;
            try
            {
                for (int item = 0; ; item++)
                {
                    token.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
            finally
            {
                sourceDisposed = true;
            }
        }

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            started.TrySetResult();
            try
            {
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
                return item;
            }
            finally
            {
                workerStopped = true;
            }
        }
    }

    [Fact]
    public async Task AlreadyCanceledToken_DoesNotStartSource()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        int calls = 0;
        await Verify.ThrowsAsync<OperationCanceledException>(() => DrainAsync(
            RangeAsync(1).SelectParallelAsync((item, _) =>
            {
                calls++;
                return ValueTask.FromResult(item);
            }, 1, cancellationToken: cancellation.Token)));
        calls.Be(0);
    }

    [Fact]
    public async Task EarlyBreak_CancelsAndJoinsActiveSelector()
    {
        var siblingStarted = NewSignal();
        bool siblingStopped = false;
        using var safety = new CancellationTokenSource(Timeout);
        await BreakAsync().WaitAsync(Timeout);
        siblingStopped.BeTrue();

        async Task BreakAsync()
        {
            await foreach (int item in RangeAsync(100).SelectParallelAsync(SelectAsync, 2, 1, safety.Token))
            {
                item.Be(0);
                break;
            }
        }

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            if (item == 0)
            {
                await siblingStarted.Task.WaitAsync(token);
                return item;
            }

            siblingStarted.TrySetResult();
            try
            {
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
                return item;
            }
            finally
            {
                siblingStopped = true;
            }
        }
    }

    [Fact]
    public async Task SourceDisposalFailure_IsPropagated()
    {
        var expected = new InvalidOperationException("source disposal failed");
        await VerifyFailureAsync(DrainAsync(Source().SelectParallelAsync(
            (item, _) => ValueTask.FromResult(item), 2, 1)), expected);

        async IAsyncEnumerable<int> Source()
        {
            await Task.CompletedTask;
            try
            {
                yield return 0;
            }
            finally
            {
                throw expected;
            }
        }
    }

    [Fact]
    public async Task CancellationCallbackFailure_DoesNotReplaceSelectorFailure()
    {
        var expected = new InvalidOperationException("original selector failure");
        var registered = NewSignal();
        bool siblingStopped = false;
        using var safety = new CancellationTokenSource(Timeout);
        await VerifyFailureAsync(DrainAsync(RangeAsync(10).SelectParallelAsync(SelectAsync,
            2, 1, safety.Token)), expected);
        siblingStopped.BeTrue();

        async ValueTask<int> SelectAsync(int item, CancellationToken token)
        {
            if (item == 0)
            {
                await registered.Task.WaitAsync(token);
                throw expected;
            }

            using var registration = token.Register(() => throw new ApplicationException("callback failed"));
            registered.TrySetResult();
            try
            {
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token);
                return item;
            }
            finally
            {
                siblingStopped = true;
            }
        }
    }

    [Fact]
    public async Task IndependentSelectorCancellation_IsPropagatedAsOriginalFailure()
    {
        var expected = new OperationCanceledException("selector canceled independently");
        await Verify.ThrowsAsync<OperationCanceledException>(async () =>
        {
            try
            {
                await DrainAsync(RangeAsync(10).SelectParallelAsync<int, int>(
                    (_, _) => throw expected, 2, 1)).WaitAsync(Timeout);
            }
            catch (OperationCanceledException ex)
            {
                ReferenceEquals(ex, expected).BeTrue();
                throw;
            }
        });
    }

    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task VerifyFailureAsync(Task task, InvalidOperationException expected)
    {
        await Verify.ThrowsAsync<InvalidOperationException>(async () =>
        {
            try
            {
                await task.WaitAsync(Timeout);
            }
            catch (InvalidOperationException ex)
            {
                ReferenceEquals(ex, expected).BeTrue();
                ex.StackTrace.NotNull();
                throw;
            }
        });
    }

    private static async Task<List<int>> DrainAsync(IAsyncEnumerable<int> source, CancellationToken token = default)
    {
        List<int> results = [];
        await foreach (int item in source.WithCancellation(token)) results.Add(item);
        return results;
    }

    private static async IAsyncEnumerable<int> RangeAsync(int count,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.CompletedTask;
        for (int item = 0; item < count; item++)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}
