using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Command to trigger automatic import for an Indexador.
/// </summary>
public record ImportarIndexadorCommand(int Id, string CorrelationId) : IRequest<Unit>;
