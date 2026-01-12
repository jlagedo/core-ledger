using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Account queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface IAccountQueryService
{
    /// <summary>
    ///     Gets accounts with RFC-8040 compliant filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of accounts list and total count for pagination.</returns>
    Task<(IReadOnlyList<Account> Accounts, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a report of active account counts grouped by account type.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of tuples containing type ID, type description, and active account count.</returns>
    Task<IReadOnlyList<(int TypeId, string TypeDescription, int ActiveAccountCount)>> GetActiveAccountsByTypeAsync(
        CancellationToken cancellationToken = default);
}
