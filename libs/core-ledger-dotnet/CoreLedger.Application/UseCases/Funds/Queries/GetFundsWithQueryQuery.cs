using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
///     Query to retrieve funds with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetFundsWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<FundDto>>;
