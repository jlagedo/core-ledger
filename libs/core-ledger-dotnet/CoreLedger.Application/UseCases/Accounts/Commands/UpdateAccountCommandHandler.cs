using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Handler for updating an existing Account.
/// </summary>
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateAccountCommandHandler> _logger;

    public UpdateAccountCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateAccountCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        UpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando Conta com ID: {AccountId}", request.Id);

        var account = await _context.Accounts.FindAsync([request.Id], cancellationToken);
        if (account == null) throw new EntityNotFoundException("Conta", request.Id);

        // Validate that the account type exists
        var accountType = await _context.AccountTypes.FindAsync([request.TypeId], cancellationToken);
        if (accountType == null) throw new EntityNotFoundException("Tipo de conta", request.TypeId);

        // Check if another account with the same code already exists
        var existing = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Code == request.Code, cancellationToken);
        if (existing != null && existing.Id != request.Id)
            throw new DomainValidationException("Conta com este código já existe");

        account.Update(
            request.Code,
            request.Name,
            request.TypeId,
            request.Status,
            request.NormalBalance);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Conta atualizada com ID: {AccountId}", request.Id);
    }
}