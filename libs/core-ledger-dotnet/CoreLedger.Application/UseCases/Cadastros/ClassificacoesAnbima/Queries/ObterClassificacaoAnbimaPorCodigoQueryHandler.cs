using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Handler for ObterClassificacaoAnbimaPorCodigoQuery.
/// </summary>
public class ObterClassificacaoAnbimaPorCodigoQueryHandler
    : IRequestHandler<ObterClassificacaoAnbimaPorCodigoQuery, ClassificacaoAnbimaDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObterClassificacaoAnbimaPorCodigoQueryHandler> _logger;

    public ObterClassificacaoAnbimaPorCodigoQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObterClassificacaoAnbimaPorCodigoQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClassificacaoAnbimaDto?> Handle(
        ObterClassificacaoAnbimaPorCodigoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ANBIMA classification with Codigo={Codigo}", request.Codigo);

        var classificacao = await _context.ClassificacoesAnbima
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == request.Codigo, cancellationToken);

        if (classificacao == null)
        {
            _logger.LogWarning("ANBIMA classification with Codigo={Codigo} not found", request.Codigo);
            return null;
        }

        _logger.LogInformation("ANBIMA classification retrieved - Codigo: {Codigo}, Nome: {Nome}", classificacao.Codigo, classificacao.Nome);

        return _mapper.Map<ClassificacaoAnbimaDto>(classificacao);
    }
}
