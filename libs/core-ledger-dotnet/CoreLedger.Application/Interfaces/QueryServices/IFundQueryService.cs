using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy support

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Fund queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface IFundQueryService
{
    /// <summary>
    ///     Gets funds with RFC-8040 compliant filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of funds list and total count for pagination.</returns>
    Task<(IReadOnlyList<Fund> Funds, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches funds for autocomplete using PostgreSQL full-text search.
    /// </summary>
    /// <param name="searchTerm">The search term to match against fund code or name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching funds limited to 10 results.</returns>
    Task<IReadOnlyList<Fund>> AutocompleteAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);
}
