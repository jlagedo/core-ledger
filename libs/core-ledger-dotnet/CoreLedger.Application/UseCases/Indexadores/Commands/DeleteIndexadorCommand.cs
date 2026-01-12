using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Command to delete an Indexador.
/// </summary>
public record DeleteIndexadorCommand(int Id) : IRequest<Unit>;
