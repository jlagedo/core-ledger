using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;

/// <summary>
///     Command to update an existing FundoTaxa.
/// </summary>
public record UpdateTaxaCommand(
    long Id,
    decimal Percentual,
    BaseCalculoTaxa BaseCalculo,
    PeriodicidadeProvisao PeriodicidadeProvisao,
    PeriodicidadePagamento PeriodicidadePagamento,
    int? DiaPagamento = null,
    decimal? ValorMinimo = null,
    decimal? ValorMaximo = null
) : IRequest<FundoTaxaResponseDto>;
