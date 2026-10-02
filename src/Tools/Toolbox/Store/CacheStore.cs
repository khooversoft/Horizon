using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Store;

public class CacheStore
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheStore> _logger;

    public CacheStore(IMemoryCache memoryCache, ILogger<CacheStore> logger)
    {
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

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

