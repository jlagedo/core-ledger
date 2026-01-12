using CoreLedger.Application.DTOs.FundoPrazo;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Prazos.Commands;

/// <summary>
///     Command to create a new FundoPrazo.
/// </summary>
public record CreatePrazoCommand(
    Guid FundoId,
    TipoPrazoOperacional TipoPrazo,
    int DiasCotizacao,
    int DiasLiquidacao,
    TimeOnly HorarioLimite,
    bool DiasUteis,
    Guid? ClasseId = null,
    int? DiasCarencia = null,
    int? CalendarioId = null,
    bool PermiteParcial = false,
    decimal? PercentualMinimo = null,
    decimal? ValorMinimo = null
) : IRequest<FundoPrazoResponseDto>;
