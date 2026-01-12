using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Fundo (cadastros) queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class FundoQueryService : IFundoQueryService
{
    private readonly ApplicationDbContext _context;

    public FundoQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Fundo> Fundos, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Fundos.AsNoTracking();

        // Apply filter
        if (!string.IsNullOrWhiteSpace(parameters.Filter))
        {
            var filterParts = parameters.Filter.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (filterParts.Length == 2)
            {
                var field = filterParts[0].Trim().ToLower();
                var value = filterParts[1].Trim().Trim('\'', '"');

                query = field switch
                {
                    "cnpj" when CNPJ.TentarCriar(value, out var cnpjFilter) => query.Where(f => f.Cnpj == cnpjFilter),
                    "cnpj" => query, // Invalid CNPJ format, return empty result
                    "razaosocial" => query.Where(f => EF.Functions.ILike(f.RazaoSocial, $"%{value}%")),
                    "nomefantasia" => query.Where(f => f.NomeFantasia != null && EF.Functions.ILike(f.NomeFantasia, $"%{value}%")),
                    "nomecurto" => query.Where(f => f.NomeCurto != null && EF.Functions.ILike(f.NomeCurto, $"%{value}%")),
                    "tipofundo" => Enum.TryParse<TipoFundo>(value, true, out var tipoFundo)
                        ? query.Where(f => f.TipoFundo == tipoFundo)
                        : query,
                    "situacao" => Enum.TryParse<SituacaoFundo>(value, true, out var situacao)
                        ? query.Where(f => f.Situacao == situacao)
                        : query,
                    "classificacaocvm" => Enum.TryParse<ClassificacaoCVM>(value, true, out var classificacao)
                        ? query.Where(f => f.ClassificacaoCVM == classificacao)
                        : query,
                    "publicoalvo" => Enum.TryParse<PublicoAlvo>(value, true, out var publicoAlvo)
                        ? query.Where(f => f.PublicoAlvo == publicoAlvo)
                        : query,
                    "exclusivo" => bool.TryParse(value, out var exclusivo)
                        ? query.Where(f => f.Exclusivo == exclusivo)
                        : query,
                    "reservado" => bool.TryParse(value, out var reservado)
                        ? query.Where(f => f.Reservado == reservado)
                        : query,
                    _ => query
                };
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var sortField = parameters.SortBy?.ToLower() ?? "createdat";
        var isDescending = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        query = sortField switch
        {
            "razaosocial" => isDescending ? query.OrderByDescending(f => f.RazaoSocial) : query.OrderBy(f => f.RazaoSocial),
            "nomefantasia" => isDescending ? query.OrderByDescending(f => f.NomeFantasia) : query.OrderBy(f => f.NomeFantasia),
            "nomecurto" => isDescending ? query.OrderByDescending(f => f.NomeCurto) : query.OrderBy(f => f.NomeCurto),
            "tipofundo" => isDescending ? query.OrderByDescending(f => f.TipoFundo) : query.OrderBy(f => f.TipoFundo),
            "situacao" => isDescending ? query.OrderByDescending(f => f.Situacao) : query.OrderBy(f => f.Situacao),
            "dataconstituicao" => isDescending ? query.OrderByDescending(f => f.DataConstituicao) : query.OrderBy(f => f.DataConstituicao),
            "datainicioatividade" => isDescending ? query.OrderByDescending(f => f.DataInicioAtividade) : query.OrderBy(f => f.DataInicioAtividade),
            "progressocadastro" => isDescending ? query.OrderByDescending(f => f.ProgressoCadastro) : query.OrderBy(f => f.ProgressoCadastro),
            "updatedat" => isDescending ? query.OrderByDescending(f => f.UpdatedAt) : query.OrderBy(f => f.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(f => f.CreatedAt) : query.OrderBy(f => f.CreatedAt)
        };

        // Apply pagination
        var fundos = await query
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync(cancellationToken);

        return (fundos, totalCount);
    }

    public async Task<IReadOnlyList<Fundo>> SearchAsync(
        string searchTerm,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Fundo>();

        var normalizedTerm = searchTerm.Trim();
        var pattern = $"%{normalizedTerm}%";

        var fundos = await _context.Fundos
            .AsNoTracking()
            .Where(f =>
                EF.Functions.ILike(f.RazaoSocial, pattern) ||
                (f.NomeFantasia != null && EF.Functions.ILike(f.NomeFantasia, pattern)) ||
                (f.NomeCurto != null && EF.Functions.ILike(f.NomeCurto, pattern)) ||
                EF.Functions.ILike(EF.Property<string>(f, "Cnpj"), pattern))
            .OrderBy(f => f.RazaoSocial)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return fundos;
    }

    public async Task<Fundo?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        if (!CNPJ.TentarCriar(cnpj, out var cnpjVO))
            return null;

        return await _context.Fundos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Cnpj == cnpjVO, cancellationToken);
    }
}
