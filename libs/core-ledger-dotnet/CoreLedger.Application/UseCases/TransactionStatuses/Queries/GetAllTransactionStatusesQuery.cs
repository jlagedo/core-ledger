using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public record GetAllTransactionStatusesQuery : IRequest<IReadOnlyList<TransactionStatusDto>>;