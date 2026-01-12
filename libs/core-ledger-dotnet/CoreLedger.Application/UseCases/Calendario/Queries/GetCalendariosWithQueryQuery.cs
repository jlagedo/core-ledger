using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Query to get calendarios with pagination, sorting, and filtering.
///     Supports individual filter parameters matching the frontend Angular application.
/// </summary>
public record GetCalendariosWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    // Individual filter parameters (matching frontend)
    string? Search = null,
    int? Praca = null,
    int? TipoDia = null,
    bool? DiaUtil = null,
    string? DataInicio = null,
    string? DataFim = null
) : IRequest<PagedResult<CalendarioDto>>;
