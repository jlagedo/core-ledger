using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public record GetAllTransactionSubTypesQuery(int? TypeId = null) : IRequest<IReadOnlyList<TransactionSubTypeDto>>;