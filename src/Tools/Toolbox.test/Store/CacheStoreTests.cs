using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.test.Store;

public class CacheStoreTests
{
    [Fact]
    public void Upsert_ShouldStoreValue()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var subject = new CacheStore(memoryCache, NullLogger<CacheStore>.Instance);

        subject.Upsert("key-1", "value-1");

        subject.TryGetValue<string>("key-1", out var value).BeTrue();
        value.Be("value-1");
    }

    [Fact]
    public void Upsert_ShouldOverwriteExistingValue()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var subject = new CacheStore(memoryCache, NullLogger<CacheStore>.Instance);

        subject.Upsert("key-1", "value-1");
        subject.Upsert("key-1", "value-2");

        subject.TryGetValue<string>("key-1", out var value).BeTrue();
        value.Be("value-2");
    }

    [Fact]
    public void Remove_ShouldDeleteExistingValue()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var subject = new CacheStore(memoryCache, NullLogger<CacheStore>.Instance);

        subject.Upsert("key-1", "value-1");
        subject.Remove("key-1");

        subject.TryGetValue<string>("key-1", out var value).BeFalse();
        value.BeNull();
    }

    [Fact]
    public async Task Upsert_ShouldHonorAbsoluteExpiration()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var subject = new CacheStore(memoryCache, NullLogger<CacheStore>.Instance);

        subject.Upsert("key-1", "value-1", DateTimeOffset.UtcNow.AddMilliseconds(50));

        subject.TryGetValue<string>("key-1", out var value).BeTrue();
        value.Be("value-1");

        await Task.Delay(200);

        subject.TryGetValue<string>("key-1", out value).BeFalse();
        value.BeNull();
    }
}
