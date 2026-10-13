using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MiniBox.Application.Common.Behaviors;

public sealed class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            logger.LogDebug("Handling {RequestName}", typeof(TRequest).Name);
            return await next();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "{RequestName} failed after {ElapsedMilliseconds} ms", typeof(TRequest).Name, timer.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            timer.Stop();
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds} ms", typeof(TRequest).Name, timer.ElapsedMilliseconds);
        }
    }
}
