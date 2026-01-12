using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Models;
using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;

/// <summary>
///     Query to get historical data for an indexador with pagination, filtering, and optional date range.
/// </summary>
public record GetHistoricoIndexadorQuery(
    int IndexadorId,
    QueryParameters Parameters,
    DateOnly? DataInicio = null,
    DateOnly? DataFim = null
) : IRequest<PagedResult<HistoricoIndexadorDto>>;
