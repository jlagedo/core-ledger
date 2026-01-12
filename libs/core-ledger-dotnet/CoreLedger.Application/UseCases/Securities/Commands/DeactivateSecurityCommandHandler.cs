using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Handler for deactivating a Security.
/// </summary>
public class DeactivateSecurityCommandHandler : IRequestHandler<DeactivateSecurityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateSecurityCommandHandler> _logger;

    public DeactivateSecurityCommandHandler(
        IApplicationDbContext context,
        ILogger<DeactivateSecurityCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeactivateSecurityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Desativando Segurança com ID: {SecurityId}", request.Id);

        var security = await _context.Securities.FindAsync([request.Id], cancellationToken);
        if (security == null) throw new EntityNotFoundException("Segurança", request.Id);

        security.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Segurança desativada com ID: {SecurityId} em {DeactivatedAt}",
            request.Id, security.DeactivatedAt);
    }
}