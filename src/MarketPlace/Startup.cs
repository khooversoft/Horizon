using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Toolbox;
using Toolbox.Data;
using Toolbox.Store;

namespace MarketPlace;

public static class Startup
{
    public static IServiceCollection AddMarketPlace(this IServiceCollection services)
    {
        services.AddInMemoryStore();
        return services;
    }
}
