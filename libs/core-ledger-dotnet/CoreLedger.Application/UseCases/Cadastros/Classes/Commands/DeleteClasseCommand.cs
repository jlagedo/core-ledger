using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Commands;

/// <summary>
///     Command to soft delete a FundoClasse.
/// </summary>
public record DeleteClasseCommand(Guid Id) : IRequest<Unit>;
