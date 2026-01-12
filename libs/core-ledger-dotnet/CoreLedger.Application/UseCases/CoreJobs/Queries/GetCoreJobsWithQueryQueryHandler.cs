using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.CoreJobs.Queries;

/// <summary>
///     Handler for retrieving core jobs with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetCoreJobsWithQueryQueryHandler
    : IRequestHandler<GetCoreJobsWithQueryQuery, Application.Models.PagedResult<CoreJobDto>>
{
    private readonly ICoreJobQueryService _coreJobQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCoreJobsWithQueryQueryHandler> _logger;

    public GetCoreJobsWithQueryQueryHandler(
        ICoreJobQueryService coreJobQueryService,
        IMapper mapper,
        ILogger<GetCoreJobsWithQueryQueryHandler> logger)
    {
        _coreJobQueryService = coreJobQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<CoreJobDto>> Handle(
        GetCoreJobsWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving core jobs with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (jobs, totalCount) = await _coreJobQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var jobDtos = _mapper.Map<IReadOnlyList<CoreJobDto>>(jobs);

        var result = new Application.Models.PagedResult<CoreJobDto>(
            jobDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);

        _logger.LogInformation(
            "Retrieved {Count} core jobs out of {TotalCount} total",
            jobDtos.Count, totalCount);

        return result;
    }
}
