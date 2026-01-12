using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.AuditLogs.Queries;

/// <summary>
///     Query to retrieve audit logs with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetAuditLogsWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<AuditLogDto>>;
