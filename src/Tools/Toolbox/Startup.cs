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

    public static IServiceCollection AddFileStore(this IServiceCollection services, string storeName, Action<FileStoreBuilder> factory)
    {
        services.NotNull();
        storeName.NotEmpty();
        factory.NotNull();

        services.AddKeyedSingleton<IFileStore>(storeName, (service, _) =>
        {
            var builder = new FileStoreBuilder();
            factory(builder);
            return builder.Build(service, storeName);
        });

        return services;
    }

    public static IServiceCollection AddInMemoryStore(this IServiceCollection services)
    {
        services.AddSingleton<IFileStore, InMemoryFileStore>();
        return services;
    }
}
