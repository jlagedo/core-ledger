using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Command to soft delete a Fundo.
/// </summary>
public record DeleteFundoCommand(
    Guid Id,
    string? DeletedBy = null
) : IRequest<Unit>;
