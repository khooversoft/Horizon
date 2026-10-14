using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Store;

public class CacheStore
{
    private readonly IMemoryCache _memoryCache;

    public CacheStore(IMemoryCache memoryCache) => _memoryCache = memoryCache.NotNull();

    public void Upsert<T>(string key, T value, DateTimeOffset? expiration = null)
    {
        key.NotEmpty();

        using var cacheEntry = _memoryCache.CreateEntry(key);
        cacheEntry.Value = value;
        cacheEntry.AbsoluteExpiration = expiration;
    }

    public void Remove(string key)
    {
        key.NotEmpty();
        _memoryCache.Remove(key);
    }

    public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value)
    {
        key.NotEmpty();
        return _memoryCache.TryGetValue(key, out value);
    }
}

