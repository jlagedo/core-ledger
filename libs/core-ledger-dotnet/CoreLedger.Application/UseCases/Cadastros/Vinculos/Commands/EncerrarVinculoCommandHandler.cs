using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Vinculos.Commands;

/// <summary>
///     Handler for EncerrarVinculoCommand.
/// </summary>
public class EncerrarVinculoCommandHandler : IRequestHandler<EncerrarVinculoCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EncerrarVinculoCommandHandler> _logger;

    public EncerrarVinculoCommandHandler(
        IApplicationDbContext context,
        ILogger<EncerrarVinculoCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(EncerrarVinculoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Encerrando vínculo com ID {Id}", request.Id);

        var vinculo = await _context.FundoVinculos
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.DataFim == null, cancellationToken);

        if (vinculo == null)
        {
            throw new EntityNotFoundException("Vínculo", request.Id);
        }

        // Encerrar using domain method
        vinculo.Encerrar(request.DataFim);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vínculo {Id} encerrado com sucesso", request.Id);

        return Unit.Value;
    }
}
