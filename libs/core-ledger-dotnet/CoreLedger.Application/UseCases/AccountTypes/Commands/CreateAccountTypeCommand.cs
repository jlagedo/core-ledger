using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Command to create a new AccountType.
/// </summary>
public record CreateAccountTypeCommand(string Description) : IRequest<AccountTypeDto>;