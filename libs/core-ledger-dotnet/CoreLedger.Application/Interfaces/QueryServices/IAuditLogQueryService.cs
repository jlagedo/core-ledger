using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex AuditLog queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface IAuditLogQueryService
{
    /// <summary>
    ///     Gets audit logs with RFC-8040 compliant filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of audit logs list and total count for pagination.</returns>
    Task<(IReadOnlyList<AuditLog> AuditLogs, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}
