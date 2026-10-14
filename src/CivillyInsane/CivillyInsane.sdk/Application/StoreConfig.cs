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
    public const string UserStoreName = "userStore";
    public const string OrdersStoreName = "ordersStore";
}


public static class StoreConfigTool
{
    public static IFileStore GetStore(this IServiceProvider services, string storeName)
    {
        var store = services.GetKeyedService<IFileStore>(storeName);
        if (store is null) throw new InvalidOperationException($"File store '{storeName}' not found.");

        return store;
    }
}
