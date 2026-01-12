using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Indexador queries with filtering, sorting, and pagination.
/// </summary>
public interface IIndexadorQueryService
{
    /// <summary>
    ///     Gets indexadores with filtering, sorting, and pagination.
    ///     Supports multiple simultaneous filters matching the frontend Angular application.
    ///     Includes aggregated historico data (último valor, última data, count).
    /// </summary>
    /// <param name="parameters">Query parameters including filters, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of indexadores list with historico stats and total count for pagination.</returns>
    Task<(IReadOnlyList<IndexadorListProjection> Indexadores, int TotalCount)> GetWithQueryAsync(
        IndexadorQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
