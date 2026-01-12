using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for Calendario complex operations.
/// </summary>
public class CalendarioQueryService : ICalendarioQueryService
{
    private readonly IApplicationDbContext _context;

    public CalendarioQueryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Calendario> Calendarios, int TotalCount)> GetWithQueryAsync(
        CalendarioQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // Build WHERE clause with multiple filters using parameterized queries
        var conditions = new List<string>();
        var sqlParameters = new List<object>();

        // Search filter (case-insensitive substring match on descricao)
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            conditions.Add($"c.descricao ILIKE {{{sqlParameters.Count}}}");
            sqlParameters.Add($"%{parameters.Search}%");
        }

        // Praca filter
        if (parameters.Praca.HasValue)
        {
            conditions.Add($"c.praca = {{{sqlParameters.Count}}}");
            sqlParameters.Add(parameters.Praca.Value);
        }

        // TipoDia filter
        if (parameters.TipoDia.HasValue)
        {
            conditions.Add($"c.tipo_dia = {{{sqlParameters.Count}}}");
            sqlParameters.Add(parameters.TipoDia.Value);
        }

        // DiaUtil filter
        if (parameters.DiaUtil.HasValue)
        {
            conditions.Add($"c.dia_util = {{{sqlParameters.Count}}}");
            sqlParameters.Add(parameters.DiaUtil.Value);
        }

        // DataInicio filter (date range start - inclusive)
        if (parameters.DataInicio.HasValue)
        {
            conditions.Add($"c.data >= {{{sqlParameters.Count}}}");
            sqlParameters.Add(parameters.DataInicio.Value);
        }

        // DataFim filter (date range end - inclusive)
        if (parameters.DataFim.HasValue)
        {
            conditions.Add($"c.data <= {{{sqlParameters.Count}}}");
            sqlParameters.Add(parameters.DataFim.Value);
        }

        // Build WHERE clause from conditions
        var whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        // Build ORDER BY clause
        var direction = parameters.SortDirection.ToLower() == "desc" ? "DESC" : "ASC";
        var orderByClause = (parameters.SortBy?.ToLower() ?? "data") switch
        {
            "data" => $"ORDER BY c.data {direction}",
            "tipodia" => $"ORDER BY c.tipo_dia {direction}",
            "tipodiadescricao" => $"ORDER BY c.tipo_dia {direction}",
            "praca" => $"ORDER BY c.praca {direction}",
            "pracadescricao" => $"ORDER BY c.praca {direction}",
            "diautil" => $"ORDER BY c.dia_util {direction}",
            "descricao" => $"ORDER BY c.descricao {direction}",
            "createdat" => $"ORDER BY c.created_at {direction}",
            _ => $"ORDER BY c.data {direction}"
        };

        // Execute count query
        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM calendarios c
            {whereClause}";

        var totalCount = 0;
        if (sqlParameters.Count > 0)
        {
            totalCount = await _context.Database
                .SqlQueryRaw<int>(countSql, sqlParameters.ToArray())
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            totalCount = await _context.Database
                .SqlQueryRaw<int>(countSql)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Add pagination parameters
        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        // Execute data query
        var dataSql = $@"
            SELECT c.*
            FROM calendarios c
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var calendarios = await _context.Calendarios
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (calendarios, totalCount);
    }

    public async Task<Calendario?> GetByDataAndPracaAsync(
        DateOnly data,
        Praca praca,
        CancellationToken cancellationToken = default)
    {
        return await _context.Calendarios
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Data == data && c.Praca == praca, cancellationToken);
    }

    public async Task<bool> IsDiaUtilAsync(
        DateOnly data,
        Praca praca,
        CancellationToken cancellationToken = default)
    {
        var calendario = await GetByDataAndPracaAsync(data, praca, cancellationToken);

        // If no calendar entry exists, assume it IS a business day
        // Calendar only contains non-business days (holidays, weekends, etc)
        return calendario?.DiaUtil ?? true;
    }

    public async Task<DateOnly> GetProximoDiaUtilAsync(
        DateOnly data,
        Praca praca,
        CancellationToken cancellationToken = default)
    {
        const int maxDaysToSearch = 30; // Safety limit to prevent infinite loops
        var currentDate = data.AddDays(1); // Start from the next day
        var daysSearched = 0;

        while (daysSearched < maxDaysToSearch)
        {
            var isDiaUtil = await IsDiaUtilAsync(currentDate, praca, cancellationToken);

            if (isDiaUtil)
            {
                return currentDate;
            }

            currentDate = currentDate.AddDays(1);
            daysSearched++;
        }

        // If no business day found within 30 days, throw exception
        throw new InvalidOperationException(
            $"No business day found within {maxDaysToSearch} days after {data:yyyy-MM-dd} for praca {praca}. " +
            "Calendar may be incomplete.");
    }

    public async Task<DateOnly> CalcularDMaisAsync(
        DateOnly data,
        int dias,
        Praca praca,
        CancellationToken cancellationToken = default)
    {
        if (dias < 0)
        {
            throw new ArgumentException("Number of business days must be non-negative", nameof(dias));
        }

        // D+0 means the same day if it's a business day, otherwise next business day
        if (dias == 0)
        {
            var isCurrentDayUtil = await IsDiaUtilAsync(data, praca, cancellationToken);
            return isCurrentDayUtil ? data : await GetProximoDiaUtilAsync(data, praca, cancellationToken);
        }

        const int maxDaysToSearch = 365; // Safety limit (1 year)
        var currentDate = data.AddDays(1); // Start from the next day
        var businessDaysCounted = 0;
        var daysSearched = 0;

        while (businessDaysCounted < dias && daysSearched < maxDaysToSearch)
        {
            var isDiaUtil = await IsDiaUtilAsync(currentDate, praca, cancellationToken);

            if (isDiaUtil)
            {
                businessDaysCounted++;

                // If we've counted enough business days, return this date
                if (businessDaysCounted == dias)
                {
                    return currentDate;
                }
            }

            currentDate = currentDate.AddDays(1);
            daysSearched++;
        }

        // If we couldn't find enough business days within the limit
        throw new InvalidOperationException(
            $"Could not calculate D+{dias} from {data:yyyy-MM-dd} for praca {praca}. " +
            $"Only found {businessDaysCounted} business days within {maxDaysToSearch} days. " +
            "Calendar may be incomplete.");
    }
}
