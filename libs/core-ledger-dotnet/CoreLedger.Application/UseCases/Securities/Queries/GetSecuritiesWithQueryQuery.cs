using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Query to retrieve securities with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetSecuritiesWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<SecurityDto>>;
