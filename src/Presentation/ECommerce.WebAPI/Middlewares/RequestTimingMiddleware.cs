using System.Diagnostics;
using ECommerce.Application.Common.Logging;

namespace ECommerce.WebAPI.Middlewares;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IECommerceLogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, IECommerceLogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogWarning("Request {Method} {Path} finished in {ElapsedMilliseconds}ms. IsAuthenticated: {IsAuthenticated}", 
                context.Request.Method,
                context.Request.Path, 
                stopwatch.ElapsedMilliseconds,
                context.User.Identity?.IsAuthenticated ?? false);
        }
    }
} 