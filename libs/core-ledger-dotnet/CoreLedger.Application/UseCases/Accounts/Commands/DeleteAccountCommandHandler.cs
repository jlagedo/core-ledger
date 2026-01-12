using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Handler for deleting an Account.
/// </summary>
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteAccountCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        DeleteAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Excluindo Conta com ID: {AccountId}", request.Id);

        var account = await _context.Accounts.FindAsync([request.Id], cancellationToken);
        if (account == null) throw new EntityNotFoundException("Conta", request.Id);

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Conta excluída com ID: {AccountId}", request.Id);
    }
}