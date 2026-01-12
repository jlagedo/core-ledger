using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Models;

/// <summary>
///     Pagination and filter query parameters for Indexador list endpoint.
///     Supports individual filter parameters matching the frontend Angular application.
/// </summary>
public record IndexadorPaginationParameters(
    [FromQuery] int Limit = 100,
    [FromQuery] int Offset = 0,
    [FromQuery] string? SortBy = null,
    [FromQuery] string SortDirection = "asc",
    // Search term (free-text search across codigo/nome)
    [FromQuery] string? Filter = null,
    // Individual filter parameters (matching frontend preset filters)
    [FromQuery] int? Tipo = null,
    [FromQuery] int? Periodicidade = null,
    [FromQuery] string? Fonte = null,
    [FromQuery] bool? Ativo = null,
    [FromQuery] bool? ImportacaoAutomatica = null);
