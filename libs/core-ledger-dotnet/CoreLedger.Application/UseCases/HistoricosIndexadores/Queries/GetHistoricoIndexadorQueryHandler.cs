using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Models;
using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;

/// <summary>
///     Handler for GetHistoricoIndexadorQuery.
///     Uses IHistoricoIndexadorQueryService for filtered, paginated results.
/// </summary>
public class GetHistoricoIndexadorQueryHandler : IRequestHandler<GetHistoricoIndexadorQuery, PagedResult<HistoricoIndexadorDto>>
{
    private readonly IHistoricoIndexadorQueryService _queryService;
    private readonly IMapper _mapper;

    public GetHistoricoIndexadorQueryHandler(IHistoricoIndexadorQueryService queryService, IMapper mapper)
    {
        _queryService = queryService;
        _mapper = mapper;
    }

    public async Task<PagedResult<HistoricoIndexadorDto>> Handle(GetHistoricoIndexadorQuery request, CancellationToken cancellationToken)
    {
        var (historicos, totalCount) = await _queryService.GetByIndexadorIdAsync(
            request.IndexadorId,
            request.Parameters,
            request.DataInicio,
            request.DataFim,
            cancellationToken);

        var dtos = historicos.Select(_mapper.Map<HistoricoIndexadorDto>).ToList();

        return new PagedResult<HistoricoIndexadorDto>(
            dtos,
            totalCount,
            request.Parameters.Limit,
            request.Parameters.Offset);
    }
}
