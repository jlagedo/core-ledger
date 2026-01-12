using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public record GetAllTransactionTypesQuery : IRequest<IReadOnlyList<TransactionTypeDto>>;