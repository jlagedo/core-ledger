using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public record GetTransactionTypeByIdQuery(int Id) : IRequest<TransactionTypeDto>;