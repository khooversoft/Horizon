using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CivillyInsane.sdk.Application;
using CivillyInsane.sdk.Models.Product;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace CivillyInsane.sdk.Actors;

public class ProductStoreActor
{
    private const int _maxParallelism = 8;
    private readonly IFileStore _fileStore;
    private readonly ConcurrentHashSet<string> _productIdCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private bool _cacheInitialized;

    public ProductStoreActor([FromKeyedServices(StoreConfig.ProductStoreName)] IFileStore fileStore)
    {
        _fileStore = fileStore.NotNull();
    }

    public async Task<bool> Delete(string productId)
    {
        string path = BuildPath(productId);
        await _cacheLock.WaitAsync();
        try
        {
            var result = await _fileStore.Delete(path);
            if (result.IsOk()) _productIdCache.Remove(productId);
            return result.IsOk();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<Option<ProductDetailRecord>> Get(string productId)
    {
        productId.NotEmpty();

        string path = BuildPath(productId);
        await _cacheLock.WaitAsync();
        try
        {
            var result = await _fileStore.Get(path).MapAsync<ProductDetailRecord>();
            if (result.IsOk()) _productIdCache.Add(productId);
            return result;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<Option<DataETag>> Get(string productId, string file)
    {
        productId.NotEmpty();
        file.NotEmpty();

        string path = BuildPath(productId, file);
        return await _fileStore.Get(path);
    }

    public async Task<IReadOnlyList<ProductDetailRecord>> GetAll()
    {
        string[] productIds;
        await _cacheLock.WaitAsync();
        try
        {
            if (!_cacheInitialized)
            {
                var entries = await _fileStore.Search("**/*.product.json");
                var ids = entries.Select(x => x.Path[..x.Path.LastIndexOf('/')]).ToArray();
                _productIdCache.Clear();
                foreach (string productId in ids) _productIdCache.Add(productId);
                _cacheInitialized = true;
            }

            productIds = _productIdCache.ToArray();
        }
        finally
        {
            _cacheLock.Release();
        }

        var list = new List<ProductDetailRecord>();

        await foreach (var record in productIds.ToAsyncEnumerable().SelectParallelAsync(
            async (productId, _) =>
            {
                var dataETag = (await _fileStore.Get(BuildPath(productId))).BeOk().Return();
                return dataETag.ToObject<ProductDetailRecord>();
            }, _maxParallelism))
        {
            list.Add(record);
        }

        return list.ToImmutableArray();
    }

    public async Task<Option> Exists(string productId)
    {
        string path = BuildPath(productId);
        return await _fileStore.Exists(path);
    }

    public async Task<Option> Exists(string productId, string file)
    {
        string path = BuildPath(productId, file);
        return await _fileStore.Exists(path);
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false)
    {
        return await _fileStore.Search(pattern, includeFolder);
    }

    public async Task<Option<string>> Upsert(ProductDetailRecord record)
    {
        var e = record.Validate();
        if (e.IsError()) return e.ToOptionStatus<string>();

        var data = record.NotNull().ToDataETag();
        return await UpsertProduct(record.ProductId, data);
    }

    public async Task<Option<string>> Upsert(string productId, string file, DataETag data)
    {
        productId.NotEmpty();
        file.NotEmpty();

        string path = $"{productId}/{file}";
        if (string.Equals(path, BuildPath(productId), StringComparison.OrdinalIgnoreCase))
        {
            return await UpsertProduct(productId, data);
        }

        var result = await _fileStore.Upsert(path, data);
        return result;
    }

    private async Task<Option<string>> UpsertProduct(string productId, DataETag data)
    {
        await _cacheLock.WaitAsync();
        try
        {
            var result = await _fileStore.Upsert(BuildPath(productId), data);
            if (result.IsOk()) _productIdCache.Add(productId);
            return result;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public string BuildPath(string productId) => $"{productId.NotEmpty()}/{BuildSafePath.Build(productId, "product", "json")}";
    public string BuildPath(string productId, string file) => $"{productId.NotEmpty()}/{file.NotEmpty()}";
}
