using System.Diagnostics;

namespace MiniBox.API.Middlewares;

public sealed class PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger, Services.PerformanceMetrics metrics)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        try { await next(context); }
        finally
        {
            timer.Stop();
            metrics.Record(context.Request.Method, context.Response.StatusCode, timer.Elapsed);
            var message = "HTTP {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms. TraceId: {TraceId}";
            if (context.Response.StatusCode >= 500) logger.LogError(message, context.Request.Method, context.Request.Path, context.Response.StatusCode, timer.ElapsedMilliseconds, context.TraceIdentifier);
            else if (context.Response.StatusCode >= 400) logger.LogWarning(message, context.Request.Method, context.Request.Path, context.Response.StatusCode, timer.ElapsedMilliseconds, context.TraceIdentifier);
            else logger.LogInformation(message, context.Request.Method, context.Request.Path, context.Response.StatusCode, timer.ElapsedMilliseconds, context.TraceIdentifier);
        }
    }
}
