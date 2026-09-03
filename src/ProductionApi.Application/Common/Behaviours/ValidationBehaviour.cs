using FluentValidation;
using MediatR;
using ValidationException = ProductionApi.Application.Common.Exceptions.ValidationException;

namespace ProductionApi.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var applicable = validators.ToArray();
        if (applicable.Length == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(applicable.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(result => result.Errors).Where(failure => failure is not null).ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
