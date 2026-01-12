using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;

/// <summary>
///     Handler for DeleteTaxaCommand.
/// </summary>
public class DeleteTaxaCommandHandler : IRequestHandler<DeleteTaxaCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteTaxaCommandHandler> _logger;

    public DeleteTaxaCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteTaxaCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTaxaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Desativando taxa com ID {Id}", request.Id);

        var taxa = await _context.FundoTaxas
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.Ativa, cancellationToken);

        if (taxa == null)
        {
            throw new EntityNotFoundException("Taxa", request.Id);
        }

        // Deactivate using domain method
        taxa.Desativar();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Taxa {Id} desativada com sucesso", request.Id);

        return Unit.Value;
    }
}
