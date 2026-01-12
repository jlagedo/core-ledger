using CoreLedger.Application.DTOs.FundoPrazo;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Prazos.Commands;

/// <summary>
///     Command to update an existing FundoPrazo.
/// </summary>
public record UpdatePrazoCommand(
    long Id,
    int DiasCotizacao,
    int DiasLiquidacao,
    TimeOnly HorarioLimite,
    bool DiasUteis,
    int? DiasCarencia = null,
    int? CalendarioId = null,
    bool PermiteParcial = false,
    decimal? PercentualMinimo = null,
    decimal? ValorMinimo = null
) : IRequest<FundoPrazoResponseDto>;
