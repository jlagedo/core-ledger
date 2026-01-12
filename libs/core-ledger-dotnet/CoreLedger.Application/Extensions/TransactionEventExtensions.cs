using CoreLedger.Application.Events;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Métodos de extensão para a entidade Transação para suportar criação de eventos de domínio.
/// </summary>
public static class TransactionEventExtensions
{
    /// <summary>
    ///     Mapeia uma entidade Transação para TransactionCreatedEvent com dados desnormalizados.
    ///     Requer que propriedades de navegação (Fund, Security, TransactionSubType, Status) estejam carregadas.
    /// </summary>
    /// <param name="transaction">A entidade de transação a mapear.</param>
    /// <param name="correlationId">ID de correlação opcional para rastreamento distribuído.</param>
    /// <param name="requestId">ID de solicitação opcional para rastreamento de solicitação.</param>
    /// <returns>Um TransactionCreatedEvent com dados desnormalizados da transação e suas relações.</returns>
    public static TransactionCreatedEvent ToTransactionCreatedEvent(
        this Transaction transaction,
        string? correlationId = null,
        string? requestId = null) =>
        new()
        {
            TransactionId = transaction.Id,
            FundId = transaction.FundId,
            FundCode = transaction.Fund?.Code ?? string.Empty,
            FundName = transaction.Fund?.Name ?? string.Empty,
            SecurityId = transaction.SecurityId,
            SecurityTicker = transaction.Security?.Ticker,
            SecurityName = transaction.Security?.Name,
            TransactionSubTypeId = transaction.TransactionSubTypeId,
            TransactionSubTypeDescription = transaction.TransactionSubType?.ShortDescription ?? string.Empty,
            TransactionTypeId = transaction.TransactionSubType?.TypeId ?? 0,
            TransactionTypeDescription = transaction.TransactionSubType?.Type?.ShortDescription ?? string.Empty,
            TradeDate = transaction.TradeDate,
            SettleDate = transaction.SettleDate,
            Quantity = transaction.Quantity,
            Price = transaction.Price,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            StatusId = transaction.StatusId,
            StatusDescription = transaction.Status?.ShortDescription ?? string.Empty,
            CreatedAt = transaction.CreatedAt,
            CreatedByUserId = transaction.CreatedByUserId,
            CorrelationId = correlationId,
            RequestId = requestId,
            OccurredOn = DateTime.UtcNow
        };
}
