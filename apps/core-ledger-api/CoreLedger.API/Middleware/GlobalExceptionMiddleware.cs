using System.Net;
using System.Text.Json;
using CoreLedger.Domain.Exceptions;
using FluentValidation;

namespace CoreLedger.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();
        var traceId = context.TraceIdentifier;

        // Use appropriate log level based on exception type
        // - Validation errors and not-found are expected business scenarios (Warning)
        // - External service errors are infrastructure issues (Error)
        // - Unknown exceptions are true errors (Error)
        LogException(exception, correlationId, traceId);

        var (statusCode, errorCode, message, errors) = exception switch
        {
            ValidationException validationException =>
                (HttpStatusCode.BadRequest,
                    "ERR-VALIDATION-001",
                    "One or more validation errors occurred",
                    validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())),

            ArgumentException argumentException =>
                (HttpStatusCode.BadRequest,
                    "ERR-VALIDATION-002",
                    argumentException.Message,
                    null),

            EntityNotFoundException notFound =>
                (HttpStatusCode.NotFound, notFound.ErrorCode, notFound.Message, null),

            DomainValidationException validation =>
                (HttpStatusCode.BadRequest, validation.ErrorCode, validation.Message, null),

            ExternalServiceException externalService =>
                (HttpStatusCode.ServiceUnavailable,
                    externalService.ErrorCode,
                    $"{externalService.ServiceName} is currently unavailable. Please try again later.",
                    null),

            DomainException domain =>
                (HttpStatusCode.BadRequest, domain.ErrorCode, domain.Message, null),

            _ => (HttpStatusCode.InternalServerError, "ERR-INTERNAL-001",
                "An internal server error occurred", (Dictionary<string, string[]>?)null)
        };

        var response = new ErrorResponse(
            errorCode,
            _environment.IsDevelopment() ? message : GetSafeMessage(exception),
            correlationId,
            errors,
            _environment.IsDevelopment() ? traceId : null
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private void LogException(Exception exception, string? correlationId, string traceId)
    {
        switch (exception)
        {
            case ValidationException:
            case DomainValidationException:
            case EntityNotFoundException:
                // Expected business scenarios - log as Warning
                _logger.LogWarning(exception,
                    "Business exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
                    correlationId, traceId);
                break;

            case ExternalServiceException:
                // Infrastructure issue - log as Error
                _logger.LogError(exception,
                    "External service exception. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
                    correlationId, traceId);
                break;

            default:
                // Unexpected exception - log as Error
                _logger.LogError(exception,
                    "Unhandled exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
                    correlationId, traceId);
                break;
        }
    }

    private static string GetSafeMessage(Exception exception)
    {
        return exception is DomainException
            ? exception.Message
            : "An error occurred while processing your request. Please contact support with the correlation ID.";
    }
}

public record ErrorResponse(
    string ErrorCode,
    string Message,
    string? CorrelationId,
    Dictionary<string, string[]>? Errors = null,
    string? TraceId = null
);

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}