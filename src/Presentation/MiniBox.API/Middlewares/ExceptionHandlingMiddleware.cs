using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MiniBox.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (FluentValidation.ValidationException exception)
        {
            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Request validation failed.",
                Instance = context.Request.Path
            };
            problem.Extensions["errors"] = exception.Errors.GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            problem.Extensions["traceId"] = context.TraceIdentifier;
            logger.LogWarning("Validation failed for {Method} {Path}. TraceId: {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Сталася внутрішня помилка. Передайте Trace ID службі підтримки.",
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
        }
    }
}
