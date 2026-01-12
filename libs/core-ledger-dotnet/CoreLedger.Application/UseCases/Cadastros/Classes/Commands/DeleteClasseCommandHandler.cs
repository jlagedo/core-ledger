using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Commands;

/// <summary>
///     Handler for DeleteClasseCommand.
/// </summary>
public class DeleteClasseCommandHandler : IRequestHandler<DeleteClasseCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteClasseCommandHandler> _logger;

    public DeleteClasseCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteClasseCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteClasseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Excluindo classe com ID {Id}", request.Id);

        var classe = await _context.FundoClasses
            .Include(c => c.Subclasses)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletedAt == null, cancellationToken);

        if (classe == null)
        {
            throw new EntityNotFoundException("Classe", request.Id);
        }

        // Soft delete using domain method (validates subclasses)
        classe.Excluir();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Classe {Id} excluída com sucesso (soft delete)", request.Id);

        return Unit.Value;
    }
}
