using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Queries;

/// <summary>
///     Query to retrieve indexadores with filtering, sorting, and pagination.
///     Supports both free-text search (Filter) and structured filters (Tipo, Ativo, etc.)
/// </summary>
public record GetIndexadoresWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter,
    int? Tipo = null,
    int? Periodicidade = null,
    string? Fonte = null,
    bool? Ativo = null,
    bool? ImportacaoAutomatica = null
) : IRequest<Application.Models.PagedResult<IndexadorDto>>;
