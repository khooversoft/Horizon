using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class Startup
{
    public static IServiceCollection AddSqlClient<T>(this IServiceCollection services, Action<SqlOption> config)
    {
        config.NotNull();

        var option = new SqlOption();
        config(option);

        services.AddSingleton<ISqlClient<T>>(services => ActivatorUtilities.CreateInstance<SqlClient<T>>(services, option));
        return services;
    }

    public static IServiceCollection AddDatalakeStore(this IServiceCollection services, DatalakeOption datalakeOption)
    {
        datalakeOption.NotNull();
        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");

        services.TryAddSingleton(datalakeOption);
        services.TryAddSingleton<DatalakeStore>();

        return services;
    }

    public static IServiceCollection AddCacheClient(this IServiceCollection services, Func<string, string> getKey, TimeSpan cacheTime)
    {
        services.NotNull();
        getKey.NotNull();

        services.AddMemoryCache();
        services.TryAddSingleton<ICacheClient>(services => ActivatorUtilities.CreateInstance<CacheClient>(services, getKey, cacheTime));
        return services;
    }

    public static IServiceCollection AddCacheClient<T>(this IServiceCollection services, Func<string, string> getKey, TimeSpan cacheTime)
    {
        services.NotNull();
        getKey.NotNull();

        services.AddMemoryCache();
        services.TryAddSingleton<ICacheClient<T>>(services => ActivatorUtilities.CreateInstance<CacheClient<T>>(services, getKey, cacheTime));
        return services;
    }

    public static IServiceCollection AddDatalakeFileStore(this IServiceCollection services, DatalakeOption datalakeOption)
    {
        datalakeOption.NotNull();
        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");

        services.AddDatalakeStore(datalakeOption);

        services.AddSingleton<IFileStore>(services =>
        {
            var datalakeStore = ActivatorUtilities.CreateInstance<DatalakeStore>(services, datalakeOption);
            return ActivatorUtilities.CreateInstance<DatalakeFileStore>(services, datalakeStore);
        });

        return services;
    }

    public static IServiceCollection AddInMemoryStore(this IServiceCollection services)
    {
        services.AddSingleton<IFileStore, InMemoryFileStore>();
        return services;
    }

    public static IServiceCollection AddFileStoreNamespace(this IServiceCollection services, string fileNamespace, string key)
    {
        fileNamespace.NotEmpty();
        StorePathTool.IsPathValid(fileNamespace).Assert(x => x, $"Invalid namespace path: {fileNamespace}");
        key.NotEmpty();

        services.AddKeyedSingleton<IFileStore>(key, (services, _) =>
        {
            var backingStore = services.GetRequiredService<IFileStore>();
            return ActivatorUtilities.CreateInstance<FileStoreNamespace>(services, fileNamespace, backingStore);
        });

        return services;
    }

    public static IServiceCollection AddCacheStore(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<CacheStore>();
        return services;
    }
}
