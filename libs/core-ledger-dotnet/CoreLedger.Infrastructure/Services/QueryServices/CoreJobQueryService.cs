using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex CoreJob queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class CoreJobQueryService : ICoreJobQueryService
{
    private readonly ApplicationDbContext _context;

    public CoreJobQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<CoreJob> Jobs, int TotalCount)> GetWithQueryAsync(
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
                    "referenceId" => $"WHERE j.reference_id ILIKE {{{sqlParameters.Count}}}",
                    "status" => $"WHERE j.status = {{{sqlParameters.Count}}}",
                    "jobDescription" => $"WHERE j.job_description ILIKE {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "referenceId" || field == "jobDescription")
                        sqlParameters.Add($"%{value}%");
                    else if (field == "status" && Enum.TryParse(typeof(JobStatus), value, true, out var statusEnum))
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
                "referenceid" => $"ORDER BY j.reference_id {direction}",
                "status" => $"ORDER BY j.status {direction}",
                "jobdescription" => $"ORDER BY j.job_description {direction}",
                "creationdate" => $"ORDER BY j.creation_date {direction}",
                "runningdate" => $"ORDER BY j.running_date {direction}",
                "finisheddate" => $"ORDER BY j.finished_date {direction}",
                "createdat" => $"ORDER BY j.created_at {direction}",
                "updatedat" => $"ORDER BY j.updated_at {direction}",
                _ => $"ORDER BY j.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY j.id ASC";
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM core_jobs j
            {whereClause}";

        var dataSql = $@"
            SELECT j.*
            FROM core_jobs j
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var jobs = await _context.Set<CoreJob>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (jobs, totalCount);
    }
}
