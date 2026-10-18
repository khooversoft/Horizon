using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class FileStoreCache : IFileStore, IDisposable
{
    private static readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);
    private readonly IFileStore _fileStore;
    private readonly IMemoryCache _memoryCache;
    private readonly DatalakeOption _datalakeOption;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private CancellationTokenSource _cacheInvalidation = new();
    private int _disposed;

    public FileStoreCache(IFileStore fileStore, IMemoryCache memoryCache, DatalakeOption datalakeOption)
    {
        _fileStore = fileStore.NotNull();
        _memoryCache = memoryCache.NotNull();
        _datalakeOption = datalakeOption.NotNull();
    }

    public async Task<Option<string>> Add(string path, DataETag data) => await Mutate(path, () => _fileStore.Add(path, data));

    public async Task<Option> CreateFolder(string path) => await _fileStore.CreateFolder(path);

    public async Task<Option> Delete(string path) => await Mutate(path, () => _fileStore.Delete(path));

    public async Task<Option> Exists(string key) => await _fileStore.Exists(key);

    public async Task<Option<DataETag>> Get(string path)
    {
        var key = GetCacheKey(path);
        await _cacheLock.WaitAsync();
        try
        {
            if (_memoryCache.TryGetValue(key, out DataETag? cached) && cached != null) return cached;

            var result = await _fileStore.Get(path);
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

    public async Task<IReadOnlyList<StorePathDetail>> Search(string path, bool includeFolder = false) =>
        await _fileStore.Search(path, includeFolder);

    public async Task<Option> SearchDelete(string pattern)
    {
        await _cacheLock.WaitAsync();
        try
        {
            return await _fileStore.SearchDelete(pattern);
        }
        finally
        {
            _cacheInvalidation.Cancel();
            _cacheInvalidation.Dispose();
            _cacheInvalidation = new CancellationTokenSource();
            _cacheLock.Release();
        }
    }

    public async Task<Option<string>> Upsert(string path, DataETag data) => await Mutate(path, () => _fileStore.Upsert(path, data));

    private async Task<T> Mutate<T>(string path, Func<Task<T>> action)
    {
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

    private (FileStoreCache, string) GetCacheKey(string path) => (this, _datalakeOption.WithBasePath(path.NotEmpty()));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

        _cacheInvalidation.Cancel();
        _cacheInvalidation.Dispose();
        _cacheLock.Dispose();
    }
}
