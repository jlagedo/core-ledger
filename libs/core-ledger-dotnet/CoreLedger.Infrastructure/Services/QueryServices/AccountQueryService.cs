using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Account queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class AccountQueryService : IAccountQueryService
{
    private readonly ApplicationDbContext _context;

    public AccountQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Account> Accounts, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // Build the WHERE clause for filtering
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
                    "code" => $"WHERE a.code = {{{sqlParameters.Count}}}",
                    "name" => $"WHERE a.name ILIKE {{{sqlParameters.Count}}}",
                    "typeId" => $"WHERE a.type_id = {{{sqlParameters.Count}}}",
                    "status" => $"WHERE a.status = {{{sqlParameters.Count}}}",
                    "normalBalance" => $"WHERE a.normal_balance = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "name")
                        sqlParameters.Add($"%{value}%");
                    else if (field == "code" && long.TryParse(value, out var codeValue))
                        sqlParameters.Add(codeValue);
                    else if (field == "typeId" && int.TryParse(value, out var intValue))
                        sqlParameters.Add(intValue);
                    else if (field == "status" && Enum.TryParse(typeof(AccountStatus), value, true, out var statusEnum))
                        sqlParameters.Add((int)statusEnum!);
                    else if (field == "normalBalance" &&
                             Enum.TryParse(typeof(NormalBalance), value, true, out var balanceEnum))
                        sqlParameters.Add((int)balanceEnum!);
                    else
                        whereClause = string.Empty;
                }
            }
        }

        // Build the ORDER BY clause
        var orderByClause = string.Empty;
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "code" => $"ORDER BY a.code {direction}",
                "name" => $"ORDER BY a.name {direction}",
                "typeid" => $"ORDER BY a.type_id {direction}",
                "status" => $"ORDER BY a.status {direction}",
                "normalbalance" => $"ORDER BY a.normal_balance {direction}",
                "createdat" => $"ORDER BY a.created_at {direction}",
                "updatedat" => $"ORDER BY a.updated_at {direction}",
                _ => $"ORDER BY a.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY a.id ASC";
        }

        // Parameterize pagination values
        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        // Build the final SQL queries
        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM accounts a
            {whereClause}";

        var dataSql = $@"
            SELECT a.*
            FROM accounts a
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        // Execute count query to get total without pagination
        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        // Get accounts using FromSqlRaw with EF Core materialization
        var accounts = await _context.Set<Account>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .Include(a => a.Type)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (accounts, totalCount);
    }

    public async Task<IReadOnlyList<(int TypeId, string TypeDescription, int ActiveAccountCount)>>
        GetActiveAccountsByTypeAsync(CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                at.id AS TypeId,
                at.description AS TypeDescription,
                COUNT(a.id)::int AS ActiveAccountCount
            FROM account_types at
            LEFT JOIN accounts a ON a.type_id = at.id AND a.status = 1
            GROUP BY at.id, at.description
            ORDER BY at.description";

        var results = await _context.Database
            .SqlQueryRaw<AccountsByTypeReportResult>(sql)
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.TypeId, r.TypeDescription, r.ActiveAccountCount)).ToList();
    }

    private class AccountsByTypeReportResult
    {
        public int TypeId { get; set; }
        public string TypeDescription { get; set; } = string.Empty;
        public int ActiveAccountCount { get; set; }
    }
}
