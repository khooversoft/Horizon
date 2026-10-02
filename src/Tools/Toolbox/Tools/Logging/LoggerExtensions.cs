using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Toolbox.Tools;

public static class LoggerExtensions
{
    public static string ToSafeLoggingFormat(this string subject) => (subject ?? string.Empty).Replace("{", "{{").Replace("}", "}}");

    public static IHostBuilder AddDebugLogging(this IHostBuilder hostBuilder, Action<string> logOutput)
    {
        hostBuilder.ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => logOutput(x)));
        return hostBuilder;
    }
}
