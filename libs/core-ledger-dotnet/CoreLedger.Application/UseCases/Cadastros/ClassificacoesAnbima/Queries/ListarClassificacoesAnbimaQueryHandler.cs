using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Handler for ListarClassificacoesAnbimaQuery.
/// </summary>
public class ListarClassificacoesAnbimaQueryHandler
    : IRequestHandler<ListarClassificacoesAnbimaQuery, ListarClassificacoesAnbimaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ListarClassificacoesAnbimaQueryHandler> _logger;

    public ListarClassificacoesAnbimaQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ListarClassificacoesAnbimaQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ListarClassificacoesAnbimaResponse> Handle(
        ListarClassificacoesAnbimaQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Listing ANBIMA classifications with ClassificacaoCvm={ClassificacaoCvm}, Nivel1={Nivel1}, Ativo={Ativo}",
            request.ClassificacaoCvm ?? "none", request.Nivel1 ?? "none", request.Ativo);

        var query = _context.ClassificacoesAnbima.AsQueryable();

        if (request.Ativo)
            query = query.Where(c => c.Ativo);

        if (!string.IsNullOrEmpty(request.ClassificacaoCvm))
            query = query.Where(c => c.ClassificacaoCvm == request.ClassificacaoCvm);

        if (!string.IsNullOrEmpty(request.Nivel1))
            query = query.Where(c => c.Nivel1 == request.Nivel1);

        var items = await query
            .AsNoTracking()
            .OrderBy(c => c.OrdemExibicao)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(c => _mapper.Map<ClassificacaoAnbimaDto>(c)).ToList();

        // Check if the filter resulted in no items due to CVM classification without ANBIMA mapping
        string? mensagem = null;
        if (dtos.Count == 0 && !string.IsNullOrEmpty(request.ClassificacaoCvm))
        {
            mensagem = $"Classificação CVM '{request.ClassificacaoCvm}' não possui classificações ANBIMA correspondentes";
        }

        _logger.LogInformation(
            "Retrieved {Count} ANBIMA classifications",
            dtos.Count);

        return new ListarClassificacoesAnbimaResponse(
            dtos,
            dtos.Count,
            request.ClassificacaoCvm,
            mensagem
        );
    }
}
