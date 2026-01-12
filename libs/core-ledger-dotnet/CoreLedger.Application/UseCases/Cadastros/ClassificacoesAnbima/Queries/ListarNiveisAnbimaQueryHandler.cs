using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Handler for ListarNiveisAnbimaQuery.
/// </summary>
public class ListarNiveisAnbimaQueryHandler
    : IRequestHandler<ListarNiveisAnbimaQuery, NiveisClassificacaoAnbimaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ListarNiveisAnbimaQueryHandler> _logger;

    public ListarNiveisAnbimaQueryHandler(
        IApplicationDbContext context,
        ILogger<ListarNiveisAnbimaQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NiveisClassificacaoAnbimaResponse> Handle(
        ListarNiveisAnbimaQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting hierarchical levels for ANBIMA classifications with ClassificacaoCvm={ClassificacaoCvm}",
            request.ClassificacaoCvm ?? "none");

        var query = _context.ClassificacoesAnbima
            .Where(c => c.Ativo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.ClassificacaoCvm))
            query = query.Where(c => c.ClassificacaoCvm == request.ClassificacaoCvm);

        var classificacoes = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Group and count Nivel1
        var nivel1Counts = classificacoes
            .GroupBy(c => c.Nivel1)
            .Select(g => new NivelContagem(g.Key, g.Count()))
            .OrderBy(nc => nc.Valor)
            .ToList();

        // Group Nivel2 by Nivel1
        var nivel2PorNivel1 = classificacoes
            .GroupBy(c => c.Nivel1)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(c => c.Nivel2)
                    .Select(n2 => new NivelContagem(n2.Key, n2.Count()))
                    .OrderBy(nc => nc.Valor)
                    .ToList()
            );

        _logger.LogInformation(
            "Retrieved hierarchical levels - Nivel1 count: {Nivel1Count}, Total classifications: {TotalCount}",
            nivel1Counts.Count, classificacoes.Count);

        return new NiveisClassificacaoAnbimaResponse(nivel1Counts, nivel2PorNivel1);
    }
}
