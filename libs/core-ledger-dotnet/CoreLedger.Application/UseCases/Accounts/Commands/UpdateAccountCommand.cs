using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Command to update an existing Account.
/// </summary>
public record UpdateAccountCommand(
    int Id,
    long Code,
    string Name,
    int TypeId,
    AccountStatus Status,
    NormalBalance NormalBalance
) : IRequest;