using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Data;

public class CacheClient : ICacheClient
{
    private readonly IMemoryCache _memoryCache;
    private readonly Func<string, string> _getKey;
    private readonly ILogger _logger;
    private readonly MemoryCacheEntryOptions _memoryOption;

    public CacheClient(Func<string, string> getKey, TimeSpan cacheTime, IMemoryCache memoryCache, ILogger<CacheClient> logger)
    {
        _getKey = getKey.NotNull();
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();

        cacheTime.Assert(x => x > TimeSpan.Zero, "Cache time must be greater than zero");
        _memoryOption = new MemoryCacheEntryOptions { SlidingExpiration = cacheTime };
    }

    public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value)
    {
        string cacheKey = BuildKey(key);

        if (_memoryCache.TryGetValue(cacheKey, out value))
        {
            value.NotNull();
            _logger.LogDebug("Cache hit, key={key}", cacheKey);
            return true;
        }

        _logger.LogDebug("Cache miss, key={key}", cacheKey);
        return false;
    }

    public void Upsert<T>(string key, T value)
    {
        value.NotNull();
        string cacheKey = BuildKey(key);

        _memoryCache.Set(cacheKey, value, _memoryOption);
        _logger.LogDebug("Cache upsert, type={type}, key={key}", typeof(T).Name, cacheKey);
    }

    public void Remove(string key)
    {
        string cacheKey = BuildKey(key);

        _memoryCache.Remove(cacheKey);
        _logger.LogDebug("Cache remove, key={key}", cacheKey);
    }

    private string BuildKey(string key) => _getKey(key.NotEmpty());
}

public class CacheClient<T>(Func<string, string> getKey, TimeSpan cacheTime, IMemoryCache memoryCache, ILogger<CacheClient<T>> logger)
    : CacheClient(getKey, cacheTime, memoryCache, logger), ICacheClient<T>;
