using MediatR;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Command to update an existing AccountType.
/// </summary>
public record UpdateAccountTypeCommand(int Id, string Description) : IRequest;