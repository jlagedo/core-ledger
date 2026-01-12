using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Command to create a new Account.
/// </summary>
public record CreateAccountCommand(
    long Code,
    string Name,
    int TypeId,
    AccountStatus Status,
    NormalBalance NormalBalance,
    string CreatedByUserId
) : IRequest<AccountDto>;