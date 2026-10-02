using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Tools;

public static class StopwatchScopeTool
{
    public static StopwatchScopeLog LogDuration(this ILogger logger, string metricName, string? message = null, params object?[] args)
    {
        return new StopwatchScopeLog(logger, metricName, message, args);
    }
}

public readonly struct StopwatchScopeLog : IDisposable
{
    private readonly ILogger? _logger;
    private readonly string? _metricName;
    private readonly string? _message = null;
    private readonly object?[] _args = [];

    public StopwatchScopeLog(ILogger logger, string metricName, string? message = null, params object?[] args)
    {
        _logger = logger;
        _metricName = metricName.NotEmpty();
        _message = message;
        _args = args.NotNull();
        Timestamp = Stopwatch.GetTimestamp();
    }

    public long Timestamp { get; }
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(Timestamp);
    public void Dispose() => Log("Dispose");

    public TimeSpan Log(string? tag = null)
    {
        string name = _metricName.NotNull() + (tag == null ? string.Empty : "." + tag);
        string msg = _message ?? nameof(StopwatchScopeLog);

        _logger?.LogDebug("message={message}, metric:{metricName}, value={value}ms", _message, name, Elapsed.TotalMilliseconds);
        return Elapsed;
    }
}
