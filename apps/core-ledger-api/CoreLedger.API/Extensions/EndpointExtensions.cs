namespace CoreLedger.API.Extensions;

/// <summary>
///     Extension methods for common endpoint operations.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    ///     Extracts the user ID from the 'sub' claim.
    /// </summary>
    public static string? GetUserId(this HttpContext context)
    {
        return context.User.FindFirst("sub")?.Value;
    }

    /// <summary>
    ///     Extracts the correlation ID set by CorrelationIdMiddleware.
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items["CorrelationId"]?.ToString();
    }

    /// <summary>
    ///     Extracts the Bearer token from the Authorization header.
    /// </summary>
    public static string? GetBearerToken(this HttpContext context)
    {
        return context.Request.Headers.Authorization
            .FirstOrDefault()?
            .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
    }
}
