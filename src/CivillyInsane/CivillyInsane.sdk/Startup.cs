using CivillyInsane.sdk.Actors;
using CivillyInsane.sdk.Application;
using CivillyInsane.sdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Store;

namespace CivillyInsane.sdk;

public static class Startup
{
    public static IServiceCollection AddCivillyInsaneServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<IAuthAccess, FakeAuthAccess>();
        services.TryAddSingleton<ProductStoreActor>();
        services.TryAddSingleton<ImageApiService>();
        services.TryAddSingleton<ShoppingCartService>();

        services.AddFileStore(StoreConfig.ProductStoreName, s => s.UseDatalake().WithCache().WithNamespace(StoreConfig.ProductStoreName));

        return services;
    }
}
