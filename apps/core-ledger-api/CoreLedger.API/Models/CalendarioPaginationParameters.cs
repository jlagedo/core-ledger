using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Models;

/// <summary>
///     Pagination and filter query parameters for Calendario list endpoint.
///     Supports individual filter parameters matching the frontend Angular application.
/// </summary>
public record CalendarioPaginationParameters(
    [FromQuery] int Limit = 100,
    [FromQuery] int Offset = 0,
    [FromQuery] string? SortBy = null,
    [FromQuery] string SortDirection = "asc",
    // Individual filter parameters (matching frontend)
    [FromQuery] string? Search = null,
    [FromQuery] int? Praca = null,
    [FromQuery] int? TipoDia = null,
    [FromQuery] bool? DiaUtil = null,
    [FromQuery] string? DataInicio = null,
    [FromQuery] string? DataFim = null);
