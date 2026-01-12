using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Command to update an existing Calendario entry.
/// </summary>
public record UpdateCalendarioCommand(
    int Id,
    TipoDia TipoDia,
    string? Descricao
) : IRequest;
