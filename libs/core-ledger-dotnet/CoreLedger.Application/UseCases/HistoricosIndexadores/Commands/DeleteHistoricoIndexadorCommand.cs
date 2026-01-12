using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;

/// <summary>
///     Command to delete a HistoricoIndexador record by ID.
/// </summary>
public record DeleteHistoricoIndexadorCommand(long Id) : IRequest<Unit>;
