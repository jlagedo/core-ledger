using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Query to get the next business day after a given date.
/// </summary>
public record GetProximoDiaUtilQuery(
    DateOnly Data,
    Praca Praca
) : IRequest<DateOnly>;
