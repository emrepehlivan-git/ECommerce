using System.Diagnostics;
using Serilog.Context;

namespace ECommerce.Infrastructure.Logging;

public sealed class SerilogLogger<T>(Serilog.ILogger logger) : Application.Common.Logging.IECommerceLogger<T>
{
    private static void EnrichWithActivityInfo(Action logAction)
    {
        var activity = Activity.Current;
        using (LogContext.PushProperty("TraceId", activity?.TraceId.ToString() ?? "none"))
        using (LogContext.PushProperty("SpanId", activity?.SpanId.ToString() ?? "none"))
        {
            logAction();
        }
    }
    
    public void LogInformation(string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Information(message, args));
    }

    public void LogWarning(string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Warning(message, args));
    }

    public void LogError(string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Error(message, args));
    }

    public void LogDebug(string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Debug(message, args));
    }

    public void LogCritical(string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Fatal(message, args));
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Error(exception, message, args));
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        EnrichWithActivityInfo(() => logger.Fatal(exception, message, args));
    }
}
