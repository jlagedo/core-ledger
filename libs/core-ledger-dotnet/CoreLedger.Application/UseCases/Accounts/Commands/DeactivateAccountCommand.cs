using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Command to deactivate an existing Account.
/// </summary>
public record DeactivateAccountCommand(int Id) : IRequest;