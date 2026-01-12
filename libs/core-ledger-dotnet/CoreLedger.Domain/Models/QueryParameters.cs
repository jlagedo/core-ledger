using System.Text.RegularExpressions;

namespace CoreLedger.Domain.Models;

/// <summary>
///     Query parameters for RFC-8040 compliant GET operations with comprehensive validation.
/// </summary>
public class QueryParameters
{
    private string? _filter;
    private int _limit = 100;
    private int _offset;
    private string _sortDirection = "asc";

    /// <summary>
    ///     Maximum number of items to return (hard limit: 100, minimum: 1).
    ///     Automatically clamped to valid range [1, 100].
    /// </summary>
    public int Limit
    {
        get => _limit;
        set => _limit = value < 1 ? 100 : Math.Min(value, 100);
    }

    /// <summary>
    ///     Number of items to skip (for pagination).
    ///     Automatically clamped to minimum of 0.
    /// </summary>
    public int Offset
    {
        get => _offset;
        set => _offset = Math.Max(value, 0);
    }

    /// <summary>
    ///     Field to sort by. Only whitelisted fields in repositories are used.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    ///     Sort direction (asc or desc). Automatically normalized to lowercase.
    ///     Invalid values default to "asc".
    /// </summary>
    public string SortDirection
    {
        get => _sortDirection;
        set => _sortDirection = ValidateSortDirection(value);
    }

    /// <summary>
    ///     Filter expression (simple field=value format).
    ///     Validated to prevent SQL injection attempts.
    /// </summary>
    public string? Filter
    {
        get => _filter;
        set => _filter = ValidateFilter(value);
    }

    /// <summary>
    ///     Fields to include in response (comma-separated).
    /// </summary>
    public string? Fields { get; set; }

    private static string ValidateSortDirection(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "asc";

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "asc" => "asc",
            "desc" => "desc",
            _ => "asc" // Default to ascending for invalid values
        };
    }

    private static string? ValidateFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // Check for SQL injection patterns
        var dangerousPatterns = new[]
        {
            @"('|(--)|;|\/\*|\*\/|xp_|sp_|exec|execute|declare|create|drop|alter|insert|update|delete|union|select|cast|convert)",
            @"(@@|@[a-z]+)", // SQL Server variables
            @"(\bor\b|\band\b).*=.*", // OR/AND with equals (potential SQLi)
            @"(<script|<iframe|javascript:|onerror=|onload=)" // XSS attempts
        };

        foreach (var pattern in dangerousPatterns)
            if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                throw new ArgumentException($"Filter contains potentially dangerous pattern: {pattern}");

        // Ensure filter follows field=value format
        if (!Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*.+$"))
            throw new ArgumentException("Filter must be in 'field=value' format with valid field name");

        return value.Trim();
    }
}

/// <summary>
///     Paged result wrapper for RFC-8040 compliance.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Limit,
    int Offset
);