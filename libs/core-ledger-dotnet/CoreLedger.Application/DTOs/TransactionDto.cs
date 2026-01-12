namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade Transação.
/// </summary>
public record TransactionDto(
    int Id,
    int FundId,
    string FundCode,
    string FundName,
    int? SecurityId,
    string? SecurityTicker,
    string? SecurityName,
    int TransactionSubTypeId,
    string TransactionSubTypeDescription,
    int TransactionTypeId,
    string TransactionTypeDescription,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId,
    string StatusDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
///     DTO para criar uma nova Transação.
/// </summary>
public record CreateTransactionDto(
    int FundId,
    int? SecurityId,
    int TransactionSubTypeId,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    Guid? IdempotencyKey);

/// <summary>
///     DTO para atualizar uma Transação existente.
/// </summary>
public record UpdateTransactionDto(
    int FundId,
    int? SecurityId,
    int TransactionSubTypeId,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId);