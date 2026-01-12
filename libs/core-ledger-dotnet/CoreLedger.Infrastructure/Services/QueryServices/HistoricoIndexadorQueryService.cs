using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for HistoricoIndexador queries with filtering by indexador_id, sorting, and pagination.
/// </summary>
public class HistoricoIndexadorQueryService : IHistoricoIndexadorQueryService
{
    private readonly ApplicationDbContext _context;

    public HistoricoIndexadorQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<HistoricoIndexador> Historicos, int TotalCount)> GetByIndexadorIdAsync(
        int indexadorId,
        QueryParameters parameters,
        DateOnly? dataInicio = null,
        DateOnly? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var sqlParameters = new List<object> { indexadorId };
        var whereConditions = new List<string> { "h.indexador_id = {0}" };

        // Add date range filters
        if (dataInicio.HasValue)
        {
            whereConditions.Add($"h.data_referencia >= {{{sqlParameters.Count}}}");
            sqlParameters.Add(dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            whereConditions.Add($"h.data_referencia <= {{{sqlParameters.Count}}}");
            sqlParameters.Add(dataFim.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", whereConditions)}";

        var orderByClause = string.Empty;
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "datareferencia" => $"ORDER BY h.data_referencia {direction}",
                "valor" => $"ORDER BY h.valor {direction}",
                "fatordiario" => $"ORDER BY h.fator_diario {direction}",
                "variacaopercentual" => $"ORDER BY h.variacao_percentual {direction}",
                "createdat" => $"ORDER BY h.created_at {direction}",
                _ => $"ORDER BY h.data_referencia {direction}"
            };
        }
        else
        {
            // Default: most recent date first
            orderByClause = "ORDER BY h.data_referencia DESC";
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM historicos_indexadores h
            {whereClause}";

        var dataSql = $@"
            SELECT h.*
            FROM historicos_indexadores h
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var historicos = await _context.Set<HistoricoIndexador>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .Include(h => h.Indexador)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (historicos, totalCount);
    }
}
