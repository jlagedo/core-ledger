using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Handler for retrieving securities with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetSecuritiesWithQueryQueryHandler
    : IRequestHandler<GetSecuritiesWithQueryQuery, Application.Models.PagedResult<SecurityDto>>
{
    private readonly ISecurityQueryService _securityQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSecuritiesWithQueryQueryHandler> _logger;

    public GetSecuritiesWithQueryQueryHandler(
        ISecurityQueryService securityQueryService,
        IMapper mapper,
        ILogger<GetSecuritiesWithQueryQueryHandler> logger)
    {
        _securityQueryService = securityQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<SecurityDto>> Handle(
        GetSecuritiesWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving securities with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (securities, totalCount) = await _securityQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var securityDtos = _mapper.Map<IReadOnlyList<SecurityDto>>(securities);

        var result = new Application.Models.PagedResult<SecurityDto>(
            securityDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);

        _logger.LogInformation(
            "Retrieved {Count} securities out of {TotalCount} total",
            securityDtos.Count, totalCount);

        return result;
    }
}
