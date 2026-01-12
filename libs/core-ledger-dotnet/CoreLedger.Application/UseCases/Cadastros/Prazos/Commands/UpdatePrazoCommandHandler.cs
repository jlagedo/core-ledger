using AutoMapper;
using CoreLedger.Application.DTOs.FundoPrazo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Prazos.Commands;

/// <summary>
///     Handler for UpdatePrazoCommand.
/// </summary>
public class UpdatePrazoCommandHandler : IRequestHandler<UpdatePrazoCommand, FundoPrazoResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePrazoCommandHandler> _logger;

    public UpdatePrazoCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UpdatePrazoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoPrazoResponseDto> Handle(UpdatePrazoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando prazo com ID {Id}", request.Id);

        var prazo = await _context.FundoPrazos
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.Ativo, cancellationToken);

        if (prazo == null)
        {
            throw new EntityNotFoundException("Prazo", request.Id);
        }

        // Update using domain method
        prazo.Atualizar(
            request.DiasCotizacao,
            request.DiasLiquidacao,
            request.HorarioLimite,
            request.DiasUteis,
            request.DiasCarencia,
            request.CalendarioId,
            request.PermiteParcial,
            request.PercentualMinimo,
            request.ValorMinimo);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Prazo {Id} atualizado com sucesso", request.Id);

        return _mapper.Map<FundoPrazoResponseDto>(prazo);
    }
}
