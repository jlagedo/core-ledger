using MediatR;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Command to delete an AccountType.
/// </summary>
public record DeleteAccountTypeCommand(int Id) : IRequest;