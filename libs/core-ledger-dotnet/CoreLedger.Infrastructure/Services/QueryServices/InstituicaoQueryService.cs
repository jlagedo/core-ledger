using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Instituicao queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class InstituicaoQueryService : IInstituicaoQueryService
{
    private readonly ApplicationDbContext _context;

    public InstituicaoQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Instituicao> Instituicoes, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        string? search = null,
        bool? ativo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Instituicoes.AsNoTracking();

        // Apply search filter (CNPJ, razão social, or nome fantasia)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
            var searchPattern = $"%{normalizedSearch}%";
            query = query.Where(i =>
                EF.Functions.ILike(EF.Property<string>(i, "Cnpj"), searchPattern) ||
                EF.Functions.ILike(i.RazaoSocial, $"%{search}%") ||
                (i.NomeFantasia != null && EF.Functions.ILike(i.NomeFantasia, $"%{search}%")));
        }

        // Apply ativo filter
        if (ativo.HasValue)
        {
            query = query.Where(i => i.Ativo == ativo.Value);
        }

        // Apply RFC-8040 filter
        if (!string.IsNullOrWhiteSpace(parameters.Filter))
        {
            var filterParts = parameters.Filter.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (filterParts.Length == 2)
            {
                var field = filterParts[0].Trim().ToLower();
                var value = filterParts[1].Trim().Trim('\'', '"');

                query = field switch
                {
                    "cnpj" when CNPJ.TentarCriar(value, out var cnpjFilter) => query.Where(i => i.Cnpj == cnpjFilter),
                    "cnpj" => query, // Invalid CNPJ format, no filter applied
                    "razaosocial" => query.Where(i => EF.Functions.ILike(i.RazaoSocial, $"%{value}%")),
                    "nomefantasia" => query.Where(i => i.NomeFantasia != null && EF.Functions.ILike(i.NomeFantasia, $"%{value}%")),
                    "ativo" => bool.TryParse(value, out var ativoValue)
                        ? query.Where(i => i.Ativo == ativoValue)
                        : query,
                    _ => query
                };
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var sortField = parameters.SortBy?.ToLower() ?? "razaosocial";
        var isDescending = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        query = sortField switch
        {
            "razaosocial" => isDescending ? query.OrderByDescending(i => i.RazaoSocial) : query.OrderBy(i => i.RazaoSocial),
            "nomefantasia" => isDescending ? query.OrderByDescending(i => i.NomeFantasia) : query.OrderBy(i => i.NomeFantasia),
            "ativo" => isDescending ? query.OrderByDescending(i => i.Ativo) : query.OrderBy(i => i.Ativo),
            "createdat" => isDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(i => i.UpdatedAt) : query.OrderBy(i => i.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(i => i.RazaoSocial) : query.OrderBy(i => i.RazaoSocial)
        };

        // Apply pagination
        var instituicoes = await query
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync(cancellationToken);

        return (instituicoes, totalCount);
    }

    public async Task<Instituicao?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        if (!CNPJ.TentarCriar(cnpj, out var cnpjVO))
            return null;

        return await _context.Instituicoes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Cnpj == cnpjVO, cancellationToken);
    }
}
