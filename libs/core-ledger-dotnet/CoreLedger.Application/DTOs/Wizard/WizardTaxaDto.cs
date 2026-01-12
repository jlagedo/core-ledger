using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Item de taxa do fundo no wizard.
/// </summary>
public record WizardTaxaDto(
    /// <summary>
    ///     Tipo da taxa.
    /// </summary>
    [Required(ErrorMessage = "Tipo da taxa é obrigatório.")]
    TipoTaxa TipoTaxa,

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
    ///     Periodicidade de cobrança/pagamento da taxa.
    /// </summary>
    [Required(ErrorMessage = "Forma de cobrança é obrigatória.")]
    PeriodicidadePagamento FormaCobranca,

    /// <summary>
    ///     Data de início da vigência da taxa.
    /// </summary>
    [Required(ErrorMessage = "Data de início da vigência é obrigatória.")]
    DateOnly DataInicioVigencia,

    /// <summary>
    ///     Percentual mínimo da taxa.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual mínimo deve estar entre 0 e 100.")]
    decimal? PercentualMinimo = null,

    /// <summary>
    ///     Percentual máximo da taxa.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual máximo deve estar entre 0 e 100.")]
    decimal? PercentualMaximo = null,

    /// <summary>
    ///     Data de fim da vigência da taxa.
    /// </summary>
    DateOnly? DataFimVigencia = null,

    /// <summary>
    ///     Identificador do benchmark (indexador) para taxa de performance.
    /// </summary>
    int? BenchmarkId = null,

    /// <summary>
    ///     Percentual sobre o benchmark para taxa de performance.
    /// </summary>
    [Range(0, 1000, ErrorMessage = "Percentual do benchmark deve estar entre 0 e 1000.")]
    decimal? PercentualBenchmark = null,

    /// <summary>
    ///     Indica se possui hurdle (taxa mínima para performance).
    /// </summary>
    bool PossuiHurdle = false,

    /// <summary>
    ///     Indica se possui high water mark.
    /// </summary>
    bool PossuiHighWaterMark = false,

    /// <summary>
    ///     Indica se a linha d'água é global (todos cotistas) ou individual.
    /// </summary>
    bool? LinhaDaguaGlobal = null,

    /// <summary>
    ///     Identificador da classe (opcional - taxa específica por classe).
    /// </summary>
    Guid? ClasseId = null
);
