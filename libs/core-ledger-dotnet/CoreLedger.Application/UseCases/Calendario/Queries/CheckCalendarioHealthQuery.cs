using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Query to check calendar health (CAL-002/CAL-003).
///     STUB: To be implemented later.
/// </summary>
public record CheckCalendarioHealthQuery() : IRequest<CalendarioHealthDto>;
