using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.CoreJobs.Queries;

/// <summary>
///     Query to retrieve core jobs with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetCoreJobsWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<CoreJobDto>>;
