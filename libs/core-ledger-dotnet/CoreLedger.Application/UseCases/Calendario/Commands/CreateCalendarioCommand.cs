using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Command to create a new Calendario entry.
/// </summary>
public record CreateCalendarioCommand(
    DateOnly Data,
    TipoDia TipoDia,
    Praca Praca,
    string? Descricao,
    string CreatedByUserId
) : IRequest<CalendarioDto>;
