using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Transaction queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class TransactionQueryService : ITransactionQueryService
{
    private readonly ApplicationDbContext _context;

    public TransactionQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Transaction> Transactions, int TotalCount)> GetWithQueryAsync(
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
                    "id" => $"WHERE t.id = {{{sqlParameters.Count}}}",
                    "fundId" => $"WHERE t.fund_id = {{{sqlParameters.Count}}}",
                    "securityId" => $"WHERE t.security_id = {{{sqlParameters.Count}}}",
                    "transactionSubTypeId" => $"WHERE t.transaction_subtype_id = {{{sqlParameters.Count}}}",
                    "statusId" => $"WHERE t.status_id = {{{sqlParameters.Count}}}",
                    "currency" => $"WHERE t.currency = {{{sqlParameters.Count}}}",
                    "tradeDate" => $"WHERE DATE(t.trade_date) = {{{sqlParameters.Count}}}",
                    "settleDate" => $"WHERE DATE(t.settle_date) = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "currency")
                    {
                        sqlParameters.Add(value.ToUpperInvariant());
                    }
                    else if (field == "tradeDate" || field == "settleDate")
                    {
                        if (DateTime.TryParse(value, out var dateValue))
                            sqlParameters.Add(dateValue.Date);
                        else
                            whereClause = string.Empty;
                    }
                    else if (int.TryParse(value, out var intValue))
                    {
                        sqlParameters.Add(intValue);
                    }
                    else
                    {
                        whereClause = string.Empty;
                    }
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
                "fundid" => $"ORDER BY t.fund_id {direction}",
                "securityid" => $"ORDER BY t.security_id {direction}",
                "transactionsubtypeid" => $"ORDER BY t.transaction_subtype_id {direction}",
                "statusid" => $"ORDER BY t.status_id {direction}",
                "tradedate" => $"ORDER BY t.trade_date {direction}",
                "settledate" => $"ORDER BY t.settle_date {direction}",
                "quantity" => $"ORDER BY t.quantity {direction}",
                "price" => $"ORDER BY t.price {direction}",
                "amount" => $"ORDER BY t.amount {direction}",
                "currency" => $"ORDER BY t.currency {direction}",
                "createdat" => $"ORDER BY t.created_at {direction}",
                "updatedat" => $"ORDER BY t.updated_at {direction}",
                _ => $"ORDER BY t.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY t.id ASC";
        }

        // Parameterize pagination values
        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        // Build the final SQL queries
        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM transactions t
            {whereClause}";

        var dataSql = $@"
            SELECT t.*
            FROM transactions t
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        // Execute count query to get total without pagination
        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        // Get transactions using FromSqlRaw with EF Core materialization
        var transactions = await _context.Set<Transaction>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType!)
            .ThenInclude(tst => tst.Type)
            .Include(t => t.Status)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }
}
