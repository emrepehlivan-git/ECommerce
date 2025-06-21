using System.Diagnostics;

namespace ECommerce.Infrastructure.Logging;

public sealed class SerilogLogger(Serilog.ILogger logger) : Application.Common.Logging.ILogger
{
    public void LogInformation(string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
        var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Information(enrichedMessage, allArgs);
    }

    public void LogWarning(string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
            var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Warning(enrichedMessage, allArgs);
    }

    public void LogError(string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
        var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Error(enrichedMessage, allArgs);
    }

    public void LogDebug(string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
        var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Debug(enrichedMessage, allArgs);
    }

    public void LogCritical(string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
        var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Fatal(enrichedMessage, allArgs);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
        var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Error(exception, enrichedMessage, allArgs);
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "none";
        var spanId = activity?.SpanId.ToString() ?? "none";
        
        var enrichedMessage = $"{message} | TraceId: {{TraceId}} | SpanId: {{SpanId}}";
        var allArgs = args.Concat([traceId, spanId]).ToArray();
        
        logger.Fatal(exception, enrichedMessage, allArgs);
    }
}