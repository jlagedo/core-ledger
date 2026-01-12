using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Query to calculate D+X business days from a starting date.
/// </summary>
public record CalcularDMaisQuery(
    DateOnly Data,
    int Dias,
    Praca Praca
) : IRequest<CalculoDMaisResultDto>;
