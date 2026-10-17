using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class DatalakeFileStore : IFileStore, IDisposable
{
    private static readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);
    private readonly DatalakeStore _datalakeStore;
    private readonly IMemoryCache? _memoryCache;
    private readonly DatalakeOption? _datalakeOption;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private CancellationTokenSource _cacheInvalidation = new();
    private int _disposed;

    public DatalakeFileStore(DatalakeStore datalakeStore) => _datalakeStore = datalakeStore.NotNull();

    public DatalakeFileStore(DatalakeStore datalakeStore, IMemoryCache memoryCache, DatalakeOption datalakeOption)
    {
        _datalakeStore = datalakeStore.NotNull();
        _memoryCache = memoryCache.NotNull();
        _datalakeOption = datalakeOption.NotNull();
    }

    public async Task<Option<string>> Add(string path, DataETag data) => await Mutate(path, () => _datalakeStore.Add(path, data));

    public async Task<Option> CreateFolder(string path) => await _datalakeStore.CreateFolder(path);

    public async Task<Option> Delete(string path) => await Mutate(path, () => _datalakeStore.Delete(path));

    public async Task<Option> Exists(string key) => await _datalakeStore.Exists(key);

    public async Task<Option<DataETag>> Get(string path)
    {
        if (_memoryCache == null) return await _datalakeStore.Get(path);

        var key = GetCacheKey(path);
        await _cacheLock.WaitAsync();
        try
        {
            if (_memoryCache.TryGetValue(key, out DataETag? cached) && cached != null) return cached;

            var result = await _datalakeStore.Get(path);
            if (result.IsOk() && result.HasValue)
            {
                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheLifetime)
                    .AddExpirationToken(new CancellationChangeToken(_cacheInvalidation.Token));

                _memoryCache.Set(key, result.Value, options);
            }

            return result;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string path, bool includeFolder = false)
    {
        var result = await _datalakeStore.Search(path, includeFolder: includeFolder);
        return result.Select(x => x.ConvertTo()).ToImmutableArray();
    }

    public async Task<Option> SearchDelete(string pattern)
    {
        if (_memoryCache == null) return await _datalakeStore.SearchDelete(pattern);

        await _cacheLock.WaitAsync();
        try
        {
            return await _datalakeStore.SearchDelete(pattern);
        }
        finally
        {
            _cacheInvalidation.Cancel();
            _cacheInvalidation.Dispose();
            _cacheInvalidation = new CancellationTokenSource();
            _cacheLock.Release();
        }
    }

    public async Task<Option<string>> Upsert(string path, DataETag data) => await Mutate(path, () => _datalakeStore.Upsert(path, data));

    private async Task<T> Mutate<T>(string path, Func<Task<T>> action)
    {
        if (_memoryCache == null) return await action();

        var key = GetCacheKey(path);
        await _cacheLock.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            _memoryCache.Remove(key);
            _cacheLock.Release();
        }
    }

    private (DatalakeFileStore, string) GetCacheKey(string path) => (this, _datalakeOption!.WithBasePath(path.NotEmpty()));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

        _cacheInvalidation.Cancel();
        _cacheInvalidation.Dispose();
        _cacheLock.Dispose();
    }

}