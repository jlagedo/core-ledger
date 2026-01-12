using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex CoreJob queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface ICoreJobQueryService
{
    /// <summary>
    ///     Gets core jobs with RFC-8040 compliant filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of jobs list and total count for pagination.</returns>
    Task<(IReadOnlyList<CoreJob> Jobs, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}
