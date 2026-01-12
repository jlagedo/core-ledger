using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Query to get a Calendario entry by ID.
/// </summary>
public record GetCalendarioByIdQuery(int Id) : IRequest<CalendarioDto>;
