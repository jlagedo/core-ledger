using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Handler for updating an existing AccountType.
/// </summary>
public class UpdateAccountTypeCommandHandler : IRequestHandler<UpdateAccountTypeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateAccountTypeCommandHandler> _logger;

    public UpdateAccountTypeCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateAccountTypeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        UpdateAccountTypeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando tipo de conta com ID: {AccountTypeId}", request.Id);

        var accountType = await _context.AccountTypes
            .FirstOrDefaultAsync(at => at.Id == request.Id, cancellationToken);

        if (accountType == null)
            throw new EntityNotFoundException("Tipo de conta", request.Id);

        // Check if another account type with the same description already exists
        var existing = await _context.AccountTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Description.ToLower() == request.Description.ToLower(), cancellationToken);

        if (existing != null && existing.Id != request.Id)
            throw new DomainValidationException("Tipo de conta com esta descrição já existe");

        accountType.UpdateDescription(request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tipo de conta atualizado com ID: {AccountTypeId}", request.Id);
    }
}
