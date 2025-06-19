using System.Diagnostics;

namespace ECommerce.Infrastructure.Logging;

public sealed class SerilogLogger(Serilog.ILogger logger) : Application.Common.Logging.ILogger
{
    public void LogInformation(string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Information(message + " {TraceId} {SpanId}", enrichedArgs);
    }

    public void LogWarning(string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Warning(message + " {TraceId} {SpanId}", enrichedArgs);
    }

    public void LogError(string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Error(message + " {TraceId} {SpanId}", enrichedArgs);
    }

    public void LogDebug(string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Debug(message + " {TraceId} {SpanId}", enrichedArgs);
    }

    public void LogCritical(string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Fatal(message + " {TraceId} {SpanId}", enrichedArgs);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Error(exception, message + " {TraceId} {SpanId}", enrichedArgs);
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        using var activity = Activity.Current;
        var enrichedArgs = EnrichWithTraceInfo(args, activity);
        logger.Fatal(exception, message + " {TraceId} {SpanId}", enrichedArgs);
    }

    private static object[] EnrichWithTraceInfo(object[] args, Activity? activity)
    {
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        return args.Concat(new object[] { traceId, spanId }).ToArray();
    }
}