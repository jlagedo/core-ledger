using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Command to deactivate a Security.
/// </summary>
public record DeactivateSecurityCommand(int Id) : IRequest;