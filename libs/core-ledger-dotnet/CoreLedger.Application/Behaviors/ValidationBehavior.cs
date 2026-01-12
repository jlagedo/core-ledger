using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.Behaviors;

/// <summary>
///     MediatR pipeline behavior that validates requests using FluentValidation before handling.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("ValidationBehavior invoked for {RequestType}. Validators found: {ValidatorCount}",
            typeof(TRequest).Name, _validators.Count());

        if (!_validators.Any())
        {
            _logger.LogWarning("No validators found for {RequestType}", typeof(TRequest).Name);
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            _logger.LogWarning("Validation failed for {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));
            throw new ValidationException(failures);
        }

        _logger.LogInformation("Validation passed for {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}