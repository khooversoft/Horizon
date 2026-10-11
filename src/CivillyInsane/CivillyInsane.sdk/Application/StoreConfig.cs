using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Types;

namespace CivillyInsane.sdk.Application;

public static class StoreConfig
{
    public const string ProductStoreName = "productStore";
    public const string AccountStoreName = "accountStore";
}


public static class GfsTestData
{
    private static readonly FrozenSet<string> _storeNames = new string[]
    {
        StoreConfig.ProductStoreName, StoreConfig.AccountStoreName
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static IFileStore GetStore(this IServiceProvider services, string storeName)
    {
        if (!_storeNames.Contains(storeName))
        {
            throw new InvalidOperationException($"File store '{storeName}' is not valid.");
        }

        var store = services.GetKeyedService<IFileStore>(storeName);
        if (store is null) throw new InvalidOperationException($"File store '{storeName}' not found.");

        return store;
    }
}
