using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex AuditLog queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class AuditLogQueryService : IAuditLogQueryService
{
    private readonly ApplicationDbContext _context;

    public AuditLogQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<AuditLog> AuditLogs, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var whereClause = string.Empty;
        var sqlParameters = new List<object>();

        if (!string.IsNullOrWhiteSpace(parameters.Filter))
        {
            var filterParts = parameters.Filter.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (filterParts.Length == 2)
            {
                var field = filterParts[0].Trim();
                var value = filterParts[1].Trim().Trim('\'', '"');

                whereClause = field switch
                {
                    "entityName" => $"WHERE a.entity_name ILIKE {{{sqlParameters.Count}}}",
                    "entityId" => $"WHERE a.entity_id = {{{sqlParameters.Count}}}",
                    "eventType" => $"WHERE a.event_type ILIKE {{{sqlParameters.Count}}}",
                    "performedByUserId" => $"WHERE a.performed_by_user_id = {{{sqlParameters.Count}}}",
                    "source" => $"WHERE a.source ILIKE {{{sqlParameters.Count}}}",
                    "correlationId" => $"WHERE a.correlation_id = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field is "entityName" or "eventType" or "source")
                        sqlParameters.Add($"%{value}%");
                    else
                        sqlParameters.Add(value);
                }
            }
        }

        var orderByClause = "ORDER BY a.performed_at DESC";
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "id" => $"ORDER BY a.id {direction}",
                "entityname" => $"ORDER BY a.entity_name {direction}",
                "entityid" => $"ORDER BY a.entity_id {direction}",
                "eventtype" => $"ORDER BY a.event_type {direction}",
                "performedat" => $"ORDER BY a.performed_at {direction}",
                "performedbyuserid" => $"ORDER BY a.performed_by_user_id {direction}",
                "source" => $"ORDER BY a.source {direction}",
                _ => $"ORDER BY a.performed_at {direction}"
            };
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM audit_log a
            {whereClause}";

        var dataSql = $@"
            SELECT a.*
            FROM audit_log a
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var auditLogs = await _context.Set<AuditLog>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (auditLogs, totalCount);
    }
}
