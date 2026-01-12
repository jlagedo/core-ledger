using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public record GetTransactionSubTypeByIdQuery(int Id) : IRequest<TransactionSubTypeDto>;