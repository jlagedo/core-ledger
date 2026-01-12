using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AuditLogs.Queries;

/// <summary>
///     Handler for retrieving audit logs with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetAuditLogsWithQueryQueryHandler
    : IRequestHandler<GetAuditLogsWithQueryQuery, Application.Models.PagedResult<AuditLogDto>>
{
    private readonly IAuditLogQueryService _auditLogQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuditLogsWithQueryQueryHandler> _logger;

    public GetAuditLogsWithQueryQueryHandler(
        IAuditLogQueryService auditLogQueryService,
        IMapper mapper,
        ILogger<GetAuditLogsWithQueryQueryHandler> logger)
    {
        _auditLogQueryService = auditLogQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<AuditLogDto>> Handle(
        GetAuditLogsWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving audit logs with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (auditLogs, totalCount) = await _auditLogQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var auditLogDtos = _mapper.Map<IReadOnlyList<AuditLogDto>>(auditLogs);

        var result = new Application.Models.PagedResult<AuditLogDto>(
            auditLogDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);

        _logger.LogInformation(
            "Retrieved {Count} audit logs out of {TotalCount} total",
            auditLogDtos.Count, totalCount);

        return result;
    }
}
