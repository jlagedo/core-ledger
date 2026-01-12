using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
///     Handler for updating an existing Fund.
/// </summary>
public class UpdateFundCommandHandler : IRequestHandler<UpdateFundCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateFundCommandHandler> _logger;

    public UpdateFundCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateFundCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        UpdateFundCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando Fundo com ID: {FundId}", request.Id);

        var fund = await _context.Funds.FindAsync([request.Id], cancellationToken);
        if (fund == null) throw new EntityNotFoundException("Fundo", request.Id);

        var existingWithName = await _context.Funds
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Name == request.Name, cancellationToken);
        if (existingWithName != null && existingWithName.Id != request.Id)
            throw new DomainValidationException("Fundo com este nome já existe");

        fund.Update(
            request.Code,
            request.Name,
            request.BaseCurrency,
            request.InceptionDate,
            request.ValuationFrequency);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fundo atualizado com ID: {FundId}", request.Id);
    }
}