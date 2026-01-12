using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
///     Handler for retrieving funds with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetFundsWithQueryQueryHandler
    : IRequestHandler<GetFundsWithQueryQuery, Application.Models.PagedResult<FundDto>>
{
    private readonly IFundQueryService _fundQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFundsWithQueryQueryHandler> _logger;

    public GetFundsWithQueryQueryHandler(
        IFundQueryService fundQueryService,
        IMapper mapper,
        ILogger<GetFundsWithQueryQueryHandler> logger)
    {
        _fundQueryService = fundQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<FundDto>> Handle(
        GetFundsWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving funds with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (funds, totalCount) = await _fundQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var fundDtos = _mapper.Map<IReadOnlyList<FundDto>>(funds);

        var result = new Application.Models.PagedResult<FundDto>(
            fundDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);

        _logger.LogInformation(
            "Retrieved {Count} funds out of {TotalCount} total",
            fundDtos.Count, totalCount);

        return result;
    }
}
