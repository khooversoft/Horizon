using CivillyInsane.sdk.Actors;
using CivillyInsane.sdk.Models.Product;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace CivillyInsane.sdk.test.Actors;

public class ProductStoreActorTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task GetAll_LoadsFilesWithBoundedParallelism()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        for (int i = 0; i < 20; i++)
        {
            (await actor.Upsert(Product($"product-{i}"))).BeOk();
        }

        var workersStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseReads = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int readsStarted = 0;
        store.BeforeGet = async () =>
        {
            if (Interlocked.Increment(ref readsStarted) == 8) workersStarted.TrySetResult();
            await releaseReads.Task.WaitAsync(Timeout);
        };

        Task<IReadOnlyList<ProductDetailRecord>> loading = actor.GetAll();
        try
        {
            await workersStarted.Task.WaitAsync(Timeout);
            Volatile.Read(ref readsStarted).Be(8);
        }
        finally
        {
            releaseReads.TrySetResult();
        }

        var products = await loading.WaitAsync(Timeout);
        products.Count.Be(20);
        products.Select(x => x.ProductId).Distinct().Count().Be(20);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task GetAll_ScansOnlyOnceEvenWhenEmpty(int productCount)
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        for (int i = 0; i < productCount; i++)
        {
            (await actor.Upsert(Product($"product-{i}"))).BeOk();
        }

        (await actor.GetAll()).Count.Be(productCount);
        (await actor.GetAll()).Count.Be(productCount);
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task GetAll_ScansDespitePreviouslyCachedProducts()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await store.Upsert(actor.BuildPath("first"), Product("first").ToDataETag())).BeOk();
        (await store.Upsert(actor.BuildPath("second"), Product("second").ToDataETag())).BeOk();
        (await actor.Get("first")).BeOk();
        (await actor.Upsert(Product("third"))).BeOk();

        var products = await actor.GetAll();

        products.Select(x => x.ProductId).Order().SequenceEqual(["first", "second", "third"]).BeTrue();
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task GetAll_ConcurrentInitializationScansOnlyOnce()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await actor.Upsert(Product("first"))).BeOk();
        var searchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSearch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        store.BeforeSearch = async () =>
        {
            searchStarted.TrySetResult();
            await releaseSearch.Task.WaitAsync(Timeout);
        };

        Task<IReadOnlyList<ProductDetailRecord>> first = actor.GetAll();
        Task<IReadOnlyList<ProductDetailRecord>> second;
        try
        {
            await searchStarted.Task.WaitAsync(Timeout);
            second = actor.GetAll();
            store.SearchCount.Be(1);
        }
        finally
        {
            releaseSearch.TrySetResult();
        }

        var results = await Task.WhenAll(first, second).WaitAsync(Timeout);
        results.All(x => x.Count == 1).BeTrue();
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task SuccessfulMutations_UpdateInventoryWithoutRescanning()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await actor.GetAll()).Count.Be(0);
        (await actor.Upsert(Product("first"))).BeOk();
        (await actor.Upsert(Product("second"))).BeOk();
        (await actor.Upsert(Product("second") with { ProductName = "Updated" })).BeOk();
        (await actor.Delete("first")).BeTrue();

        var products = await actor.GetAll();

        products.Count.Be(1);
        products.Single().ProductId.Be("second");
        products.Single().ProductName.Be("Updated");
        (await actor.Delete("second")).BeTrue();
        (await actor.GetAll()).Count.Be(0);
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task FailedMutations_DoNotChangeInventory()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await actor.Upsert(Product("first"))).BeOk();
        (await actor.GetAll()).Count.Be(1);
        store.FailUpsert = true;
        store.FailDelete = true;

        (await actor.Upsert(Product("second"))).IsError().BeTrue();
        (await actor.Delete("first")).BeFalse();

        var products = await actor.GetAll();
        products.Count.Be(1);
        products.Single().ProductId.Be("first");
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task FileUpserts_CacheOnlyProductFiles()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await actor.GetAll()).Count.Be(0);
        (await actor.Upsert("attachment-only", "image.png", "image".ToDataETag())).BeOk();
        (await actor.GetAll()).Count.Be(0);

        string file = StorePathTool.GetFileName(actor.BuildPath("first"));
        (await actor.Upsert("first", file, Product("first").ToDataETag())).BeOk();

        var products = await actor.GetAll();
        products.Count.Be(1);
        products.Single().ProductId.Be("first");
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task Get_AddsDiscoveredProductToInitializedCache()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await actor.GetAll()).Count.Be(0);
        (await store.Upsert(actor.BuildPath("first"), Product("first").ToDataETag())).BeOk();

        (await actor.Get("missing")).BeNotFound();
        (await actor.Get("first")).BeOk();
        (await actor.GetAll()).Single().ProductId.Be("first");
        store.SearchCount.Be(1);
    }

    [Fact]
    public async Task GetAll_FailedSearchCanBeRetried()
    {
        var store = new TrackingFileStore
        {
            BeforeSearch = () => throw new InvalidOperationException("Search failed")
        };
        var actor = new ProductStoreActor(store);

        await Verify.ThrowsAsync<InvalidOperationException>(async () => await actor.GetAll());
        store.BeforeSearch = null;
        (await actor.Upsert(Product("first"))).BeOk();

        (await actor.GetAll()).Single().ProductId.Be("first");
        store.SearchCount.Be(2);
    }

    [Fact]
    public async Task GetAll_ReadFailurePropagatesAndCanBeRetriedWithoutScanning()
    {
        var store = new TrackingFileStore();
        var actor = new ProductStoreActor(store);
        (await actor.Upsert(Product("first"))).BeOk();
        store.BeforeGet = () => throw new InvalidOperationException("Read failed");

        await Verify.ThrowsAsync<InvalidOperationException>(async () => await actor.GetAll());
        store.BeforeGet = null;

        (await actor.GetAll()).Single().ProductId.Be("first");
        store.SearchCount.Be(1);
    }

    private static ProductDetailRecord Product(string productId) => new()
    {
        ProductId = productId,
        ProductName = productId,
        Description = "Description",
        ProductType = "TeeShirt",
        Brand = "Civilly Insane",
        CurrencyCode = "USD"
    };

    private sealed class TrackingFileStore : IFileStore
    {
        private readonly InMemoryFileStore _store = new();
        private int _searchCount;

        public int SearchCount => Volatile.Read(ref _searchCount);
        public Func<Task>? BeforeGet { get; set; }
        public Func<Task>? BeforeSearch { get; set; }
        public bool FailUpsert { get; set; }
        public bool FailDelete { get; set; }

        public Task<Option<string>> Add(string path, DataETag data) => _store.Add(path, data);
        public Task<Option> CreateFolder(string path) => _store.CreateFolder(path);
        public Task<Option> Exists(string key) => _store.Exists(key);
        public Task<Option> SearchDelete(string pattern) => _store.SearchDelete(pattern);

        public Task<Option> Delete(string path) => FailDelete
            ? Task.FromResult(new Option(StatusCode.Conflict))
            : _store.Delete(path);

        public Task<Option<string>> Upsert(string path, DataETag data) => FailUpsert
            ? Task.FromResult(new Option<string>(StatusCode.Conflict))
            : _store.Upsert(path, data);

        public async Task<Option<DataETag>> Get(string path)
        {
            if (BeforeGet is not null) await BeforeGet();
            return await _store.Get(path);
        }

        public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false)
        {
            Interlocked.Increment(ref _searchCount);
            if (BeforeSearch is not null) await BeforeSearch();
            return await _store.Search(pattern, includeFolder);
        }
    }
}
