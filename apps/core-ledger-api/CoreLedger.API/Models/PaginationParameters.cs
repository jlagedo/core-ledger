using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Models;

/// <summary>
///     Common pagination query parameters for list endpoints.
/// </summary>
public record PaginationParameters(
    [FromQuery] int Limit = 100,
    [FromQuery] int Offset = 0,
    [FromQuery] string? SortBy = null,
    [FromQuery] string SortDirection = "asc",
    [FromQuery] string? Filter = null);
