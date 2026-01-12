using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

public record GetTransactionByIdQuery(int Id) : IRequest<TransactionDto>;