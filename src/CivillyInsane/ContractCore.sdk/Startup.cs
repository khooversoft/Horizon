using Microsoft.Extensions.DependencyInjection;
using Toolbox;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public static class Startup
{
    public static IServiceCollection AddContractCore(this IServiceCollection services, ContractCoreOption option)
    {
        option.NotNull().Validate().ThrowOnError();

        services.AddSingleton(option);
        services.AddSqlClient<ContractCoreStore>(c => c.ConnectionString = option.ConnectionString);
        services.AddSingleton<ContractCoreStore>();
        return services;
    }
}
