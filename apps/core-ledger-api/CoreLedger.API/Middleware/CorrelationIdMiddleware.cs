using Serilog.Context;

namespace CoreLedger.API.Middleware;

/// <summary>
///     Middleware that adds correlation ID and authenticated user information to the log context.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Extract user information from authentication context (only 'sub' claim available in access token)
        var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
        var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

        // Push all context properties to Serilog
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("IsAuthenticated", isAuthenticated))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}