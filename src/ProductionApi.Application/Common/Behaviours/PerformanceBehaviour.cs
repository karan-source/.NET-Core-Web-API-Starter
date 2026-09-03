using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProductionApi.Application.Common.Behaviours;

/// <summary>Surfaces slow handlers in the logs so regressions are caught before users report them.</summary>
public sealed class PerformanceBehaviour<TRequest, TResponse>(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long WarningThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        if (timer.ElapsedMilliseconds > WarningThresholdMilliseconds)
        {
            logger.LogWarning(
                "Slow request: {RequestName} took {ElapsedMilliseconds}ms",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds);
        }

        return response;
    }
}
