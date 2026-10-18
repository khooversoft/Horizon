using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Store;

public class FileStoreCacheTests
{
    private static readonly DatalakeOption _option = new() { BasePath = "root" };

    [Fact]
    public async Task Get_ShouldCacheSuccessfulReadsUsingNormalizedPaths()
    {
        var inner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var subject = new FileStoreCache(inner, cache, _option);
        (await inner.Upsert("file.txt", "original".ToDataETag())).BeOk();

        var first = (await subject.Get("file.txt")).BeOk().Return();
        (await inner.Upsert("file.txt", "changed".ToDataETag())).BeOk();
        var second = (await subject.Get("ROOT/FILE.TXT")).BeOk().Return();

        VerifyString.Be(second.DataToString(), first.DataToString());
        VerifyString.Be(second.ETag, first.ETag);
        VerifyInt.Be(inner.ReadCount, 1);
    }

    [Fact]
    public async Task Get_ShouldNotCacheMissingFiles()
    {
        var inner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var subject = new FileStoreCache(inner, cache, _option);

        (await subject.Get("file.txt")).BeNotFound();
        (await inner.Add("file.txt", "created".ToDataETag())).BeOk();
        var result = (await subject.Get("file.txt")).BeOk().Return();

        VerifyString.Be(result.DataToString(), "created");
        VerifyInt.Be(inner.ReadCount, 2);
    }

    [Fact]
    public async Task Upsert_ShouldInvalidateTheNormalizedPath()
    {
        var inner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var subject = new FileStoreCache(inner, cache, _option);
        (await subject.Add("file.txt", "original".ToDataETag())).BeOk();
        (await subject.Get("file.txt")).BeOk();

        (await subject.Upsert("ROOT/FILE.TXT", "updated".ToDataETag())).BeOk();
        var result = (await subject.Get("file.txt")).BeOk().Return();

        VerifyString.Be(result.DataToString(), "updated");
        VerifyInt.Be(inner.ReadCount, 2);
    }

    [Fact]
    public async Task Add_ShouldInvalidateEvenOnConflict()
    {
        var inner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var subject = new FileStoreCache(inner, cache, _option);
        (await inner.Add("file.txt", "original".ToDataETag())).BeOk();
        (await subject.Get("file.txt")).BeOk();
        (await inner.Upsert("file.txt", "changed".ToDataETag())).BeOk();

        (await subject.Add("ROOT/FILE.TXT", "conflict".ToDataETag())).BeConflict();
        var result = (await subject.Get("file.txt")).BeOk().Return();

        VerifyString.Be(result.DataToString(), "changed");
        VerifyInt.Be(inner.ReadCount, 2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Delete_ShouldInvalidateEvenWhenFileIsAlreadyMissing(bool alreadyMissing)
    {
        var inner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var subject = new FileStoreCache(inner, cache, _option);
        (await inner.Add("file.txt", "original".ToDataETag())).BeOk();
        (await subject.Get("file.txt")).BeOk();
        if (alreadyMissing) (await inner.Delete("file.txt")).BeOk();

        var result = await subject.Delete("ROOT/FILE.TXT");
        if (alreadyMissing) result.BeNotFound();
        else result.BeOk();

        (await subject.Get("file.txt")).BeNotFound();
        VerifyInt.Be(inner.ReadCount, 2);
    }

    [Fact]
    public async Task SearchDelete_ShouldInvalidateOnlyThisStoresEntriesEvenWhenNothingMatches()
    {
        var inner = new TestFileStore();
        var otherInner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var subject = new FileStoreCache(inner, cache, _option);
        using var other = new FileStoreCache(otherInner, cache, _option);
        cache.Set("unrelated", "value");
        (await inner.Add("file.txt", "original".ToDataETag())).BeOk();
        (await otherInner.Add("file.txt", "other original".ToDataETag())).BeOk();
        (await subject.Get("file.txt")).BeOk();
        (await other.Get("file.txt")).BeOk();
        (await inner.Upsert("file.txt", "changed".ToDataETag())).BeOk();
        (await otherInner.Upsert("file.txt", "other changed".ToDataETag())).BeOk();

        (await subject.SearchDelete("missing/*")).BeNotFound();

        VerifyString.Be((await subject.Get("file.txt")).BeOk().Return().DataToString(), "changed");
        VerifyString.Be((await other.Get("file.txt")).BeOk().Return().DataToString(), "other original");
        VerifyInt.Be(inner.ReadCount, 2);
        VerifyInt.Be(otherInner.ReadCount, 1);
        VerifyString.Be(cache.Get<string>("unrelated"), "value");
    }

    [Fact]
    public async Task WithCache_ShouldWrapTheConfiguredRoot()
    {
        var inner = new TestFileStore();
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton(_option);
        using var provider = services.BuildServiceProvider();
        var builder = new FileStoreBuilder().UseRoot(_ => inner).WithCache();
        using var subject = (FileStoreCache)builder.Build(provider, "test");
        (await inner.Add("file.txt", "original".ToDataETag())).BeOk();

        (await subject.Get("file.txt")).BeOk();
        (await subject.Get("file.txt")).BeOk();

        VerifyInt.Be(inner.ReadCount, 1);
    }

    [Fact]
    public void Constructor_ShouldRejectNullDependencies()
    {
        var inner = new TestFileStore();
        using var cache = new MemoryCache(new MemoryCacheOptions());

        Verify.Throws<ArgumentNullException>(() => new FileStoreCache(null!, cache, _option));
        Verify.Throws<ArgumentNullException>(() => new FileStoreCache(inner, null!, _option));
        Verify.Throws<ArgumentNullException>(() => new FileStoreCache(inner, cache, null!));
    }

    private sealed class TestFileStore : IFileStore
    {
        private readonly InMemoryFileStore _inner = new();
        public int ReadCount { get; private set; }

        public Task<Option<string>> Add(string path, DataETag data) => _inner.Add(_option.WithBasePath(path), data);
        public Task<Option> CreateFolder(string path) => _inner.CreateFolder(_option.WithBasePath(path));
        public Task<Option> Delete(string path) => _inner.Delete(_option.WithBasePath(path));
        public Task<Option> Exists(string key) => _inner.Exists(_option.WithBasePath(key));
        public Task<Option> SearchDelete(string pattern) => _inner.SearchDelete(_option.WithBasePath(pattern));
        public Task<Option<DataETag>> Get(string path)
        {
            ReadCount++;
            return _inner.Get(_option.WithBasePath(path));
        }
        public Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false) =>
            _inner.Search(_option.WithBasePath(pattern), includeFolder);
        public Task<Option<string>> Upsert(string path, DataETag data) => _inner.Upsert(_option.WithBasePath(path), data);
    }
}
