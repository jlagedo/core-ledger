using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Query to check if a date is a business day for a market location.
/// </summary>
public record CheckDiaUtilQuery(
    DateOnly Data,
    Praca Praca
) : IRequest<DiaUtilResultDto>;
