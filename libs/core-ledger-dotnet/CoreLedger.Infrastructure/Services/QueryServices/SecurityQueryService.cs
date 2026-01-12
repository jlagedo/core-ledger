using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Security queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class SecurityQueryService : ISecurityQueryService
{
    private readonly ApplicationDbContext _context;

    public SecurityQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Security> Securities, int TotalCount)> GetWithQueryAsync(
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
                    "name" => $"WHERE s.name ILIKE {{{sqlParameters.Count}}}",
                    "ticker" => $"WHERE s.ticker = {{{sqlParameters.Count}}}",
                    "type" => $"WHERE s.type = {{{sqlParameters.Count}}}",
                    "currency" => $"WHERE s.currency = {{{sqlParameters.Count}}}",
                    "status" => $"WHERE s.status = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "name")
                        sqlParameters.Add($"%{value}%");
                    else if (field == "ticker")
                        sqlParameters.Add(value.ToUpperInvariant());
                    else if (field == "type" && Enum.TryParse(typeof(SecurityType), value, true, out var typeEnum))
                        sqlParameters.Add((int)typeEnum!);
                    else if (field == "currency")
                        sqlParameters.Add(value.ToUpperInvariant());
                    else if (field == "status" &&
                             Enum.TryParse(typeof(SecurityStatus), value, true, out var statusEnum))
                        sqlParameters.Add((int)statusEnum!);
                    else
                        whereClause = string.Empty;
                }
            }
        }

        var orderByClause = string.Empty;
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "name" => $"ORDER BY s.name {direction}",
                "ticker" => $"ORDER BY s.ticker {direction}",
                "type" => $"ORDER BY s.type {direction}",
                "currency" => $"ORDER BY s.currency {direction}",
                "status" => $"ORDER BY s.status {direction}",
                "createdat" => $"ORDER BY s.created_at {direction}",
                "updatedat" => $"ORDER BY s.updated_at {direction}",
                _ => $"ORDER BY s.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY s.id ASC";
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM securities s
            {whereClause}";

        var dataSql = $@"
            SELECT s.*
            FROM securities s
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var securities = await _context.Set<Security>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (securities, totalCount);
    }

    public async Task<IReadOnlyList<Security>> AutocompleteAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        // Split on whitespace and process each term separately for tsquery
        var terms = searchTerm.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Escape special tsquery characters for each term and create prefix queries
        var tsQueryTerms = terms.Select(term =>
        {
            var escapedTerm = term
                .Replace("'", "''")
                .Replace("&", "")
                .Replace("|", "")
                .Replace("!", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(":", "")
                .Replace("*", "");
            return escapedTerm + ":*";
        });

        // Join terms with & (AND operator) for tsquery
        var tsQueryTerm = string.Join(" & ", tsQueryTerms);
        var prefixPattern = searchTerm + "%";
        var sqlParameters = new List<object> { tsQueryTerm, prefixPattern };

        var sql = @"
            SELECT s.*
            FROM securities s
            WHERE
                to_tsvector('simple', coalesce(s.ticker, '') || ' ' || coalesce(s.name, ''))
                @@ to_tsquery('simple', {0})
            ORDER BY
                CASE WHEN s.ticker ILIKE {1} THEN 1 ELSE 2 END,
                s.name ASC
            LIMIT 10";

        var securities = await _context.Set<Security>()
            .FromSqlRaw(sql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return securities;
    }
}
