using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Handler for VerificarCompatibilidadeAnbimaQuery.
/// </summary>
public class VerificarCompatibilidadeAnbimaQueryHandler
    : IRequestHandler<VerificarCompatibilidadeAnbimaQuery, VerificarCompatibilidadeResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<VerificarCompatibilidadeAnbimaQueryHandler> _logger;

    public VerificarCompatibilidadeAnbimaQueryHandler(
        IApplicationDbContext context,
        ILogger<VerificarCompatibilidadeAnbimaQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VerificarCompatibilidadeResponse> Handle(
        VerificarCompatibilidadeAnbimaQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Verifying compatibility between CodigoAnbima={CodigoAnbima} and ClassificacaoCvm={ClassificacaoCvm}",
            request.CodigoAnbima, request.ClassificacaoCvm);

        var classificacao = await _context.ClassificacoesAnbima
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == request.CodigoAnbima, cancellationToken);

        if (classificacao == null)
        {
            _logger.LogWarning("ANBIMA classification with Codigo={Codigo} not found", request.CodigoAnbima);
            return new VerificarCompatibilidadeResponse(
                false,
                request.CodigoAnbima,
                request.ClassificacaoCvm,
                $"Classificação ANBIMA '{request.CodigoAnbima}' não encontrada"
            );
        }

        var compativel = classificacao.ClassificacaoCvm == request.ClassificacaoCvm;

        string? mensagem = null;
        if (!compativel)
        {
            mensagem = $"Classificação ANBIMA '{classificacao.Nome}' não é compatível com CVM '{request.ClassificacaoCvm}'. " +
                       $"Esperado: {classificacao.ClassificacaoCvm}";
        }

        _logger.LogInformation(
            "Compatibility verified - CodigoAnbima: {CodigoAnbima}, ClassificacaoCvm: {ClassificacaoCvm}, Compatible: {Compativel}",
            request.CodigoAnbima, request.ClassificacaoCvm, compativel);

        return new VerificarCompatibilidadeResponse(
            compativel,
            request.CodigoAnbima,
            request.ClassificacaoCvm,
            mensagem
        );
    }
}
