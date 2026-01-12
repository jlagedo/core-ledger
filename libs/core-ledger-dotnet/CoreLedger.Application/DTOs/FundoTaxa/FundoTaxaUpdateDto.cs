using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoTaxa;

/// <summary>
///     DTO para atualização de uma Taxa de Fundo.
/// </summary>
public record FundoTaxaUpdateDto(
    /// <summary>
    ///     Percentual da taxa (% a.a.).
    /// </summary>
    [Required(ErrorMessage = "Percentual é obrigatório.")]
    [Range(0, 100, ErrorMessage = "Percentual deve estar entre 0 e 100.")]
    decimal Percentual,

    /// <summary>
    ///     Base de cálculo da taxa.
    /// </summary>
    [Required(ErrorMessage = "Base de cálculo é obrigatória.")]
    BaseCalculoTaxa BaseCalculo,

    /// <summary>
    ///     Periodicidade de provisão da taxa.
    /// </summary>
    [Required(ErrorMessage = "Periodicidade de provisão é obrigatória.")]
    PeriodicidadeProvisao PeriodicidadeProvisao,

    /// <summary>
    ///     Periodicidade de pagamento da taxa.
    /// </summary>
    [Required(ErrorMessage = "Periodicidade de pagamento é obrigatória.")]
    PeriodicidadePagamento PeriodicidadePagamento,

    /// <summary>
    ///     Dia do mês para pagamento (1-28).
    /// </summary>
    [Range(1, 28, ErrorMessage = "Dia de pagamento deve ser entre 1 e 28.")]
    int? DiaPagamento = null,

    /// <summary>
    ///     Valor mínimo mensal da taxa.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo não pode ser negativo.")]
    decimal? ValorMinimo = null,

    /// <summary>
    ///     Valor máximo (cap) da taxa.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor máximo não pode ser negativo.")]
    decimal? ValorMaximo = null
);
