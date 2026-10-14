using System;
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
    private readonly IFileStore _fileStore;

    public ProductStoreActor([FromKeyedServices(StoreConfig.ProductStoreName)] IFileStore fileStore)
    {
        _fileStore = fileStore.NotNull();
    }

    public async Task<bool> Delete(string productId)
    {
        string path = BuildPath(productId);
        var result = await _fileStore.Delete(path);
        return result.IsOk() ? true : false;
    }

    public async Task<Option<ProductDetailRecord>> Get(string productId)
    {
        productId.NotEmpty();

        string path = BuildPath(productId);
        return await _fileStore.Get(path).MapAsync<ProductDetailRecord>();
    }

    public async Task<Option<DataETag>> Get(string productId, string file)
    {
        productId.NotEmpty();
        file.NotEmpty();

        string path = $"{productId}/{file}";
        return await _fileStore.Get(path);
    }

    public async Task<IReadOnlyList<ProductDetailRecord>> GetAll()
    {
        var entries = await _fileStore.Search("**/*.product.json");
        var list = new List<ProductDetailRecord>();

        foreach (var entry in entries)
        {
            var dataEtag = (await _fileStore.Get(entry.Path)).BeOk().Return();
            var o = dataEtag.ToObject<ProductDetailRecord>();
            list.Add(o);
        }

        return list.ToImmutableArray();
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
        string path = BuildPath(record.ProductId);
        var result = await _fileStore.Upsert(path, data);
        return result;
    }

    public async Task<Option<string>> Upsert(string productId, string file, DataETag data)
    {
        productId.NotEmpty();
        file.NotEmpty();

        string path = $"{productId}/{file}";
        var result = await _fileStore.Upsert(path, data);
        return result;
    }

    private string BuildPath(string productId) => $"{productId.NotEmpty()}/{BuildSafePath.Build(productId, "product", "json")}";

}
