using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
///     Query to retrieve all Account items.
/// </summary>
public record GetAllAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;