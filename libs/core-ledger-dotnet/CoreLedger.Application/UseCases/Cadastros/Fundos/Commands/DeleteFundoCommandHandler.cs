using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Handler for DeleteFundoCommand.
/// </summary>
public class DeleteFundoCommandHandler : IRequestHandler<DeleteFundoCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteFundoCommandHandler> _logger;

    public DeleteFundoCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteFundoCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteFundoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Excluindo fundo com ID {Id}", request.Id);

        var fundo = await _context.Fundos
            .FirstOrDefaultAsync(f => f.Id == request.Id && f.DeletedAt == null, cancellationToken);

        if (fundo == null)
        {
            throw new EntityNotFoundException("Fundo", request.Id);
        }

        // Soft delete using domain method
        fundo.Excluir(request.DeletedBy);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fundo {Id} excluído com sucesso (soft delete)", request.Id);

        return Unit.Value;
    }
}
