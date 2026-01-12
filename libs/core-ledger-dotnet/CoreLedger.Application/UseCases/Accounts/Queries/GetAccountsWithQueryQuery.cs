using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
///     Query to retrieve accounts with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetAccountsWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<AccountDto>>;
