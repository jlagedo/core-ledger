using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

/// <summary>
///     Query to retrieve transactions with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetTransactionsWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<TransactionDto>>;
