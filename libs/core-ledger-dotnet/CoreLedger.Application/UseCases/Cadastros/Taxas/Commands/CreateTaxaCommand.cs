using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;

/// <summary>
///     Command to create a new FundoTaxa.
/// </summary>
public record CreateTaxaCommand(
    Guid FundoId,
    TipoTaxa TipoTaxa,
    decimal Percentual,
    BaseCalculoTaxa BaseCalculo,
    PeriodicidadeProvisao PeriodicidadeProvisao,
    PeriodicidadePagamento PeriodicidadePagamento,
    DateOnly DataInicioVigencia,
    Guid? ClasseId = null,
    int? DiaPagamento = null,
    decimal? ValorMinimo = null,
    decimal? ValorMaximo = null,
    FundoTaxaPerformanceCreateDto? ParametrosPerformance = null
) : IRequest<FundoTaxaResponseDto>;
