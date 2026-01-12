using MediatR;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Command to delete an Account.
/// </summary>
public record DeleteAccountCommand(int Id) : IRequest;