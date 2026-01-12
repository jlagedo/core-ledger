using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoTaxa;

/// <summary>
///     DTO para criação dos parâmetros de performance de uma taxa.
/// </summary>
public record FundoTaxaPerformanceCreateDto(
    /// <summary>
    ///     Identificador do indexador (benchmark).
    /// </summary>
    [Required(ErrorMessage = "Indexador (benchmark) é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "IndexadorId deve ser maior que zero.")]
    int IndexadorId,

    /// <summary>
    ///     Percentual do benchmark (ex: 100 para 100% CDI).
    /// </summary>
    [Required(ErrorMessage = "Percentual do benchmark é obrigatório.")]
    [Range(0.01, 1000, ErrorMessage = "Percentual do benchmark deve estar entre 0.01 e 1000.")]
    decimal PercentualBenchmark,

    /// <summary>
    ///     Método de cálculo da taxa de performance.
    /// </summary>
    [Required(ErrorMessage = "Método de cálculo é obrigatório.")]
    MetodoCalculoPerformance MetodoCalculo,

    /// <summary>
    ///     Indica se utiliza linha d'água (high water mark).
    /// </summary>
    bool LinhaDagua = true,

    /// <summary>
    ///     Periodicidade de cristalização da performance.
    /// </summary>
    [Required(ErrorMessage = "Periodicidade de cristalização é obrigatória.")]
    PeriodicidadeCristalizacao PeriodicidadeCristalizacao = PeriodicidadeCristalizacao.Semestral,

    /// <summary>
    ///     Mês de cristalização (1-12).
    /// </summary>
    [Range(1, 12, ErrorMessage = "Mês de cristalização deve ser entre 1 e 12.")]
    int? MesCristalizacao = null
);
