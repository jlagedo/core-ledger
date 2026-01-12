using MediatR;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public record UpdateTransactionCommand(
    int Id,
    int FundId,
    int? SecurityId,
    int TransactionSubTypeId,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId
) : IRequest;