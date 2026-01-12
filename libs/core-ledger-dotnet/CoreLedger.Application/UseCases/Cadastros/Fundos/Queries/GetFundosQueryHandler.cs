using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;
using AppModels = CoreLedger.Application.Models;
using DomainModels = CoreLedger.Domain.Models;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Handler for GetFundosQuery.
/// </summary>
public class GetFundosQueryHandler : IRequestHandler<GetFundosQuery, AppModels.PagedResult<FundoListDto>>
{
    private readonly IFundoQueryService _queryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFundosQueryHandler> _logger;

    public GetFundosQueryHandler(
        IFundoQueryService queryService,
        IMapper mapper,
        ILogger<GetFundosQueryHandler> logger)
    {
        _queryService = queryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AppModels.PagedResult<FundoListDto>> Handle(
        GetFundosQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting fundos with Limit={Limit}, Offset={Offset}, Filter={Filter}",
            request.Limit, request.Offset, request.Filter ?? "none");

        var parameters = new DomainModels.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (fundos, totalCount) = await _queryService.GetWithQueryAsync(parameters, cancellationToken);

        var dtos = fundos.Select(f => _mapper.Map<FundoListDto>(f)).ToList();

        _logger.LogInformation("Retrieved {Count} fundos out of {TotalCount} total", dtos.Count, totalCount);

        return new AppModels.PagedResult<FundoListDto>(dtos, totalCount, request.Limit, request.Offset);
    }
}
