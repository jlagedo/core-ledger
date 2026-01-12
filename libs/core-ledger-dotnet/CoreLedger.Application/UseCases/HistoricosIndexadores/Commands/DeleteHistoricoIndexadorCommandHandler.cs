using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;

/// <summary>
///     Handler for DeleteHistoricoIndexadorCommand.
///     Deletes a historical record by ID.
/// </summary>
public class DeleteHistoricoIndexadorCommandHandler : IRequestHandler<DeleteHistoricoIndexadorCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteHistoricoIndexadorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteHistoricoIndexadorCommand request, CancellationToken cancellationToken)
    {
        var historico = await _context.HistoricosIndexadores
            .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);

        if (historico == null)
        {
            throw new EntityNotFoundException("Histórico do Indexador", request.Id);
        }

        _context.HistoricosIndexadores.Remove(historico);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
