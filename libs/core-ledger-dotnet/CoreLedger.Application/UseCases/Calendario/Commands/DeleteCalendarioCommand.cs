using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Command to delete a Calendario entry.
/// </summary>
public record DeleteCalendarioCommand(int Id) : IRequest;
