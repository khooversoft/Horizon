using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;

namespace Toolbox.Tools;

public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Projects items concurrently, streaming results in completion order, not source order.
    /// </summary>
    /// <remarks>
    /// Both input and output buffers are bounded by <paramref name="bufferCapacity"/>,
    /// which defaults to twice <paramref name="maxParallelism"/>.
    /// Failures cancel the pipeline and propagate the first original exception after all tasks finish.
    /// Cancellation and early disposal also join all tasks. The source and selector must cooperate
    /// with their supplied cancellation token for shutdown to finish promptly.
    /// Arguments are validated when enumeration starts.
    /// </remarks>
    public static async IAsyncEnumerable<TResult> SelectParallelAsync<TSource, TResult>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<TResult>> selector,
        int maxParallelism,
        int? bufferCapacity = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        source.NotNull();
        selector.NotNull();
        maxParallelism.Assert(static x => x > 0, "maxParallelism must be greater than 0.");

        if (bufferCapacity is null)
        {
            maxParallelism.Assert(static x => x <= int.MaxValue / 2,
                "maxParallelism must not exceed int.MaxValue / 2 when bufferCapacity is omitted.");
        }

        int capacity = bufferCapacity ?? maxParallelism * 2;
        capacity.Assert(static x => x > 0, "bufferCapacity must be greater than 0.");

        using var pipelineCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        CancellationToken pipelineToken = pipelineCancellation.Token;
        cancellationToken.ThrowIfCancellationRequested();
        ExceptionDispatchInfo? failure = null;

        var inputChannel = Channel.CreateBounded<TSource>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = maxParallelism == 1
            });

        var outputChannel = Channel.CreateBounded<TResult>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = maxParallelism == 1,
                SingleReader = true
            });

        Task producer = ProduceAsync();

        Task[] workers = Enumerable
            .Range(0, maxParallelism)
            .Select(_ => ProcessAsync())
            .ToArray();

        Task completion = CompleteOutputAsync([producer, .. workers]);

        try
        {
            // Completion, rather than a canceled read, carries the original failure to the reader.
            await foreach (TResult result in outputChannel.Reader.ReadAllAsync())
            {
                if (pipelineToken.IsCancellationRequested) break;
                yield return result;
            }

            await completion;
            failure?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
        }
        finally
        {
            // Important if the caller breaks out of await foreach early.
            await CancelPipelineAsync();
            await completion;
        }

        async Task ProduceAsync()
        {
            try
            {
                await foreach (TSource item in source.WithCancellation(pipelineToken))
                {
                    await inputChannel.Writer.WriteAsync(item, pipelineToken);
                }
            }
            catch (Exception ex)
            {
                RecordFailure(ex);
                await CancelPipelineAsync();
            }
            finally
            {
                inputChannel.Writer.TryComplete();
            }
        }

        async Task ProcessAsync()
        {
            try
            {
                await foreach (TSource item in inputChannel.Reader.ReadAllAsync(pipelineToken))
                {
                    pipelineToken.ThrowIfCancellationRequested();
                    TResult result = await selector(item, pipelineToken);

                    await outputChannel.Writer.WriteAsync(result, pipelineToken);
                }
            }
            catch (Exception ex)
            {
                RecordFailure(ex);
                await CancelPipelineAsync();
            }
        }

        async Task CompleteOutputAsync(Task[] tasks)
        {
            try
            {
                // Always join every participant, including when the producer fails.
                await Task.WhenAll(tasks);
            }
            finally
            {
                outputChannel.Writer.TryComplete();
            }
        }

        void RecordFailure(Exception exception)
        {
            if (exception is OperationCanceledException && pipelineToken.IsCancellationRequested) return;
            Interlocked.CompareExchange(ref failure, ExceptionDispatchInfo.Capture(exception), null);
        }

        async Task CancelPipelineAsync()
        {
            try
            {
                await pipelineCancellation.CancelAsync();
            }
            catch (Exception ex)
            {
                // Cancellation callbacks must not prevent joining tasks or replace an earlier fault.
                RecordFailure(ex);
            }
        }
    }
}