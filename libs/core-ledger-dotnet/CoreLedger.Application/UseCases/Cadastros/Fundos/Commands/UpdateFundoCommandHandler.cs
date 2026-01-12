using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Handler for UpdateFundoCommand.
/// </summary>
public class UpdateFundoCommandHandler : IRequestHandler<UpdateFundoCommand, FundoResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFundoCommandHandler> _logger;

    public UpdateFundoCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UpdateFundoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoResponseDto> Handle(UpdateFundoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando fundo com ID {Id}", request.Id);

        var fundo = await _context.Fundos
            .FirstOrDefaultAsync(f => f.Id == request.Id && f.DeletedAt == null, cancellationToken);

        if (fundo == null)
        {
            throw new EntityNotFoundException("Fundo", request.Id);
        }

        // Update cadastral data
        fundo.AtualizarDadosCadastrais(
            request.RazaoSocial,
            request.NomeFantasia,
            request.NomeCurto,
            request.DataConstituicao,
            request.DataInicioAtividade,
            request.ClassificacaoAnbima,
            request.CodigoAnbima,
            request.UpdatedBy);

        // Update regulatory configurations if provided
        if (request.ClassificacaoCVM.HasValue ||
            request.Prazo.HasValue ||
            request.PublicoAlvo.HasValue ||
            request.Tributacao.HasValue ||
            request.Condominio.HasValue ||
            request.Exclusivo.HasValue ||
            request.Reservado.HasValue ||
            request.PermiteAlavancagem.HasValue ||
            request.AceitaCripto.HasValue ||
            request.PercentualExterior.HasValue)
        {
            fundo.AtualizarConfiguracoesRegulatorias(
                request.ClassificacaoCVM ?? fundo.ClassificacaoCVM,
                request.Prazo ?? fundo.Prazo,
                request.PublicoAlvo ?? fundo.PublicoAlvo,
                request.Tributacao ?? fundo.Tributacao,
                request.Condominio ?? fundo.Condominio,
                request.Exclusivo ?? fundo.Exclusivo,
                request.Reservado ?? fundo.Reservado,
                request.PermiteAlavancagem ?? fundo.PermiteAlavancagem,
                request.AceitaCripto ?? fundo.AceitaCripto,
                request.PercentualExterior ?? fundo.PercentualExterior,
                request.UpdatedBy);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fundo {Id} atualizado com sucesso", request.Id);

        return _mapper.Map<FundoResponseDto>(fundo);
    }
}
