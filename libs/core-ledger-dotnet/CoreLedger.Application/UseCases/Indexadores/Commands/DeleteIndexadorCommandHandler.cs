using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Handler for deleting an Indexador.
/// </summary>
public class DeleteIndexadorCommandHandler : IRequestHandler<DeleteIndexadorCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteIndexadorCommandHandler> _logger;

    public DeleteIndexadorCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteIndexadorCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        DeleteIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Excluindo indexador {Id}", request.Id);

        var indexador = await _context.Indexadores
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (indexador == null)
        {
            _logger.LogWarning("Falha na exclusão do indexador: Indexador {Id} não encontrado", request.Id);
            throw new EntityNotFoundException(nameof(Indexador), request.Id);
        }

        // IDX-002: Check if historical data exists
        var hasHistorico = await _context.HistoricosIndexadores
            .AsNoTracking()
            .AnyAsync(h => h.IndexadorId == request.Id, cancellationToken);

        if (hasHistorico)
        {
            _logger.LogWarning("Falha na exclusão do indexador: Indexador {Id} possui dados históricos", request.Id);
            throw new DomainValidationException("Indexador com histórico não pode ser excluído. Use inativação.");
        }

        _context.Indexadores.Remove(indexador);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Indexador {Id} excluído", request.Id);

        return Unit.Value;
    }
}
