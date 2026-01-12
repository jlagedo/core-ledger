using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

/// <summary>
/// Handler for ProcessTransactionCommand that validates domain rules and updates transaction status.
/// Implements full domain validation using Transaction.Update() and status transition rules.
/// </summary>
public class ProcessTransactionCommandHandler : IRequestHandler<ProcessTransactionCommand, ProcessTransactionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ProcessTransactionCommandHandler> _logger;

    private const int StatusPending = 1;
    private const int StatusExecuted = 2;
    private const int StatusFailed = 8;

    public ProcessTransactionCommandHandler(
        IApplicationDbContext context,
        ILogger<ProcessTransactionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Processes a transaction by validating domain rules and updating status.
    /// </summary>
    public async Task<ProcessTransactionResult> Handle(
        ProcessTransactionCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando transação - IdTransação: {TransactionId}, IdCorrelação: {CorrelationId}",
            request.TransactionId, request.CorrelationId);

        // 1. Fetch transaction with navigation properties
        var transaction = await _context.Transactions
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType)
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction == null)
        {
            _logger.LogWarning(
                "Transação não encontrada - IdTransação: {TransactionId}",
                request.TransactionId);
            return new ProcessTransactionResult(
                false,
                request.TransactionId,
                StatusFailed,
                string.Empty,
                "Transação não encontrada");
        }

        // 2. Validate status transition (only process Pending transactions)
        if (transaction.StatusId != StatusPending)
        {
            _logger.LogWarning(
                "Transação não está em status Pendente - IdTransação: {TransactionId}, " +
                "StatusAtual: {CurrentStatus}",
                transaction.Id, transaction.StatusId);
            return new ProcessTransactionResult(
                false,
                transaction.Id,
                transaction.StatusId,
                transaction.CreatedByUserId,
                $"Transação não está em status Pendente (atual: {transaction.StatusId})");
        }

        // 3. Perform full domain validation via Transaction.Update()
        try
        {
            _logger.LogInformation(
                "Validando regras de domínio da transação - IdTransação: {TransactionId}",
                transaction.Id);

            // Call Update with Executed status to trigger all domain validation
            transaction.Update(
                fundId: transaction.FundId,
                securityId: transaction.SecurityId,
                transactionSubTypeId: transaction.TransactionSubTypeId,
                tradeDate: transaction.TradeDate,
                settleDate: transaction.SettleDate,
                quantity: transaction.Quantity,
                price: transaction.Price,
                amount: transaction.Amount,
                currency: transaction.Currency,
                statusId: StatusExecuted);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transação processada com sucesso - IdTransação: {TransactionId}, " +
                "Status: Executada",
                transaction.Id);

            return new ProcessTransactionResult(
                true,
                transaction.Id,
                StatusExecuted,
                transaction.CreatedByUserId);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(ex,
                "Falha na validação da transação - IdTransação: {TransactionId}, " +
                "Erro: {ErrorMessage}",
                transaction.Id, ex.Message);

            // Update to Failed status on validation error
            transaction.Update(
                fundId: transaction.FundId,
                securityId: transaction.SecurityId,
                transactionSubTypeId: transaction.TransactionSubTypeId,
                tradeDate: transaction.TradeDate,
                settleDate: transaction.SettleDate,
                quantity: transaction.Quantity,
                price: transaction.Price,
                amount: transaction.Amount,
                currency: transaction.Currency,
                statusId: StatusFailed);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transação marcada como Falha - IdTransação: {TransactionId}, " +
                "Erro: {ErrorMessage}",
                transaction.Id, ex.Message);

            return new ProcessTransactionResult(
                false,
                transaction.Id,
                StatusFailed,
                transaction.CreatedByUserId,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error processing transaction - TransactionId: {TransactionId}",
                transaction.Id);
            throw;
        }
    }
}
