using AutoMapper;
using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;

/// <summary>
///     Handler for UpdateTaxaCommand.
/// </summary>
public class UpdateTaxaCommandHandler : IRequestHandler<UpdateTaxaCommand, FundoTaxaResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateTaxaCommandHandler> _logger;

    public UpdateTaxaCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UpdateTaxaCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoTaxaResponseDto> Handle(UpdateTaxaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando taxa com ID {Id}", request.Id);

        var taxa = await _context.FundoTaxas
            .Include(t => t.ParametrosPerformance)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.Ativa, cancellationToken);

        if (taxa == null)
        {
            throw new EntityNotFoundException("Taxa", request.Id);
        }

        // Update using domain method
        taxa.Atualizar(
            request.Percentual,
            request.BaseCalculo,
            request.PeriodicidadeProvisao,
            request.PeriodicidadePagamento,
            request.DiaPagamento,
            request.ValorMinimo,
            request.ValorMaximo);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Taxa {Id} atualizada com sucesso", request.Id);

        return _mapper.Map<FundoTaxaResponseDto>(taxa);
    }
}
