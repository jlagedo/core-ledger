using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Transaction queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface ITransactionQueryService
{
    /// <summary>
    ///     Gets transactions with RFC-8040 compliant filtering, sorting, and pagination.
    ///     Includes related entities: Fund, Security, TransactionSubType (with Type), and Status.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of transactions list and total count for pagination.</returns>
    Task<(IReadOnlyList<Transaction> Transactions, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}
