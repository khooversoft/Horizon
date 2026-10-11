using CivillyInsane.sdk.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox;
using Toolbox.Data;
using Toolbox.Security;

namespace CivillyInsane.sdk;

public static class Startup
{
    public static IServiceCollection AddCivillyInsaneServices(this IServiceCollection services)
    {
        services.AddCacheStore();
        services.TryAddSingleton<IAuthAccess, FakeAuthAccess>();

        services.AddKeyedSingleton<IFileStore>(StoreConfig.AccountStoreName, (service, _) =>
        {
            return ActivatorUtilities.CreateInstance<FileStoreNamespace>(service, StoreConfig.AccountStoreName);
        });

        services.AddKeyedSingleton<IFileStore>(StoreConfig.ProductStoreName, (service, _) =>
        {
            return ActivatorUtilities.CreateInstance<FileStoreNamespace>(service, StoreConfig.ProductStoreName);
        });

        return services;
    }
}
