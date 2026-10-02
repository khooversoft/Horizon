using Microsoft.AspNetCore.Routing;
using Toolbox.Razor.Frame;

namespace Toolbox.Razor;

public static class Startup
{
    public static void AddToolboxRazorMap(this IEndpointRouteBuilder endpoints)
    {
        endpoints.RegisterEndpoints();
    }
}
