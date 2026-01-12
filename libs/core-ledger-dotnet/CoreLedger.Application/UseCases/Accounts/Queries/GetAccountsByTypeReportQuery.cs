using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

public record GetAccountsByTypeReportQuery : IRequest<IReadOnlyList<AccountsByTypeReportDto>>;