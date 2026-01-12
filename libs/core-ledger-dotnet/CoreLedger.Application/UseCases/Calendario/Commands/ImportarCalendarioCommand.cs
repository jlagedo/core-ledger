using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Command to import calendar data for a year.
///     STUB: To be implemented later.
/// </summary>
public record ImportarCalendarioCommand(
    int Ano,
    Praca Praca,
    string CreatedByUserId
) : IRequest<ImportarCalendarioResultDto>;
