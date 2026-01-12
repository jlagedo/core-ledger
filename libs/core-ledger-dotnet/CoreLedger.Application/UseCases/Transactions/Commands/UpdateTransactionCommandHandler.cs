using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateTransactionCommandHandler> _logger;

    public UpdateTransactionCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateTransactionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando transação com ID: {TransactionId}", request.Id);

        var transaction = await _context.Transactions.FindAsync([request.Id], cancellationToken);
        if (transaction == null)
            throw new EntityNotFoundException("Transação", request.Id);

        // Capture old values for audit trail
        var oldAmount = transaction.Amount;
        var oldQuantity = transaction.Quantity;
        var oldPrice = transaction.Price;
        var oldCurrency = transaction.Currency;
        var oldStatusId = transaction.StatusId;

        // Validate foreign keys
        var fund = await _context.Funds.FindAsync([request.FundId], cancellationToken);
        if (fund == null)
        {
            _logger.LogWarning("Falha na atualização de transação: Fundo {FundId} não encontrado para transação {TransactionId}", request.FundId, request.Id);
            throw new EntityNotFoundException("Fundo", request.FundId);
        }

        if (request.SecurityId.HasValue)
        {
            var security = await _context.Securities.FindAsync([request.SecurityId.Value], cancellationToken);
            if (security == null)
            {
                _logger.LogWarning("Falha na atualização de transação: Segurança {SecurityId} não encontrada para transação {TransactionId}", request.SecurityId.Value, request.Id);
                throw new EntityNotFoundException("Segurança", request.SecurityId.Value);
            }
        }

        var subType = await _context.TransactionSubTypes.FindAsync([request.TransactionSubTypeId], cancellationToken);
        if (subType == null)
        {
            _logger.LogWarning("Falha na atualização de transação: SubTipo de Transação {SubTypeId} não encontrado para transação {TransactionId}", request.TransactionSubTypeId, request.Id);
            throw new EntityNotFoundException("SubTipo de Transação", request.TransactionSubTypeId);
        }

        var status = await _context.TransactionStatuses.FindAsync([request.StatusId], cancellationToken);
        if (status == null)
        {
            _logger.LogWarning("Falha na atualização de transação: Status de Transação {StatusId} não encontrado para transação {TransactionId}", request.StatusId, request.Id);
            throw new EntityNotFoundException("Status de Transação", request.StatusId);
        }

        transaction.Update(
            request.FundId,
            request.SecurityId,
            request.TransactionSubTypeId,
            request.TradeDate,
            request.SettleDate,
            request.Quantity,
            request.Price,
            request.Amount,
            request.Currency,
            request.StatusId);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated transaction {TransactionId} - Amount: {OldAmount} → {NewAmount}, " +
            "Quantity: {OldQuantity} → {NewQuantity}, Price: {OldPrice} → {NewPrice}, " +
            "Currency: {OldCurrency} → {NewCurrency}, Status: {OldStatusId} → {NewStatusId}",
            transaction.Id, oldAmount, transaction.Amount,
            oldQuantity, transaction.Quantity, oldPrice, transaction.Price,
            oldCurrency, transaction.Currency, oldStatusId, transaction.StatusId);
    }
}