using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Handler for deleting an AccountType.
/// </summary>
public class DeleteAccountTypeCommandHandler : IRequestHandler<DeleteAccountTypeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteAccountTypeCommandHandler> _logger;

    public DeleteAccountTypeCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteAccountTypeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        DeleteAccountTypeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Excluindo Tipo de Conta com ID: {AccountTypeId}", request.Id);

        var accountType = await _context.AccountTypes
            .FirstOrDefaultAsync(at => at.Id == request.Id, cancellationToken);

        if (accountType == null)
            throw new EntityNotFoundException("Tipo de conta", request.Id);

        _context.AccountTypes.Remove(accountType);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tipo de Conta excluído com ID: {AccountTypeId}", request.Id);
    }
}
