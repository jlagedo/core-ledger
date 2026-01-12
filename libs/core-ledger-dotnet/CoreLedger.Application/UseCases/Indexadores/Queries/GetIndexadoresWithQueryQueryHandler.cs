using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Queries;

/// <summary>
///     Handler for retrieving indexadores with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetIndexadoresWithQueryQueryHandler
    : IRequestHandler<GetIndexadoresWithQueryQuery, Application.Models.PagedResult<IndexadorDto>>
{
    private readonly IIndexadorQueryService _indexadorQueryService;
    private readonly ILogger<GetIndexadoresWithQueryQueryHandler> _logger;

    public GetIndexadoresWithQueryQueryHandler(
        IIndexadorQueryService indexadorQueryService,
        ILogger<GetIndexadoresWithQueryQueryHandler> logger)
    {
        _indexadorQueryService = indexadorQueryService;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<IndexadorDto>> Handle(
        GetIndexadoresWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving indexadores with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, Tipo: {Tipo}, Ativo: {Ativo}",
            request.Limit, request.Offset, request.SortBy, request.Filter, request.Tipo, request.Ativo);

        var parameters = new Domain.Models.IndexadorQueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter,
            Tipo = request.Tipo,
            Periodicidade = request.Periodicidade,
            Fonte = request.Fonte,
            Ativo = request.Ativo,
            ImportacaoAutomatica = request.ImportacaoAutomatica
        };

        var (projections, totalCount) = await _indexadorQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        // Map projections to DTOs
        var indexadorDtos = projections.Select(p => new IndexadorDto(
            p.Id,
            p.Codigo,
            p.Nome,
            p.Tipo,
            p.Tipo.ToString(),
            p.Fonte,
            p.Periodicidade,
            p.Periodicidade.ToString(),
            p.FatorAcumulado,
            p.DataBase,
            p.UrlFonte,
            p.ImportacaoAutomatica,
            p.Ativo,
            p.CreatedAt,
            p.UpdatedAt,
            p.UltimoValor,
            p.UltimaData,
            p.HistoricoCount
        )).ToList();

        return new Application.Models.PagedResult<IndexadorDto>(
            indexadorDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);
    }
}
