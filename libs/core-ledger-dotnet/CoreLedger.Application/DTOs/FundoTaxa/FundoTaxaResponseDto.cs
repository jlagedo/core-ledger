using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoTaxa;

/// <summary>
///     DTO de resposta completa de uma Taxa de Fundo.
/// </summary>
public record FundoTaxaResponseDto(
    /// <summary>
    ///     Identificador único da taxa.
    /// </summary>
    long Id,

    /// <summary>
    ///     Identificador do fundo ao qual a taxa pertence.
    /// </summary>
    Guid FundoId,

    /// <summary>
    ///     Identificador da classe (se taxa específica por classe).
    /// </summary>
    Guid? ClasseId,

    /// <summary>
    ///     Tipo da taxa.
    /// </summary>
    TipoTaxa TipoTaxa,

    /// <summary>
    ///     Descrição do tipo da taxa.
    /// </summary>
    string TipoTaxaDescricao,

    /// <summary>
    ///     Percentual da taxa (% a.a.).
    /// </summary>
    decimal Percentual,

    /// <summary>
    ///     Base de cálculo da taxa.
    /// </summary>
    BaseCalculoTaxa BaseCalculo,

    /// <summary>
    ///     Descrição da base de cálculo.
    /// </summary>
    string BaseCalculoDescricao,

    /// <summary>
    ///     Periodicidade de provisão da taxa.
    /// </summary>
    PeriodicidadeProvisao PeriodicidadeProvisao,

    /// <summary>
    ///     Descrição da periodicidade de provisão.
    /// </summary>
    string PeriodicidadeProvisaoDescricao,

    /// <summary>
    ///     Periodicidade de pagamento da taxa.
    /// </summary>
    PeriodicidadePagamento PeriodicidadePagamento,

    /// <summary>
    ///     Descrição da periodicidade de pagamento.
    /// </summary>
    string PeriodicidadePagamentoDescricao,

    /// <summary>
    ///     Dia do mês para pagamento.
    /// </summary>
    int? DiaPagamento,

    /// <summary>
    ///     Valor mínimo mensal da taxa.
    /// </summary>
    decimal? ValorMinimo,

    /// <summary>
    ///     Valor máximo (cap) da taxa.
    /// </summary>
    decimal? ValorMaximo,

    /// <summary>
    ///     Data de início da vigência da taxa.
    /// </summary>
    DateOnly DataInicioVigencia,

    /// <summary>
    ///     Data de fim da vigência da taxa.
    /// </summary>
    DateOnly? DataFimVigencia,

    /// <summary>
    ///     Indica se a taxa está ativa.
    /// </summary>
    bool Ativa,

    /// <summary>
    ///     Parâmetros específicos para taxa de performance.
    /// </summary>
    FundoTaxaPerformanceResponseDto? ParametrosPerformance,

    /// <summary>
    ///     Data de criação do registro.
    /// </summary>
    DateTime CreatedAt,

    /// <summary>
    ///     Data da última atualização do registro.
    /// </summary>
    DateTime? UpdatedAt
);

/// <summary>
///     DTO de resposta dos parâmetros de performance.
/// </summary>
public record FundoTaxaPerformanceResponseDto(
    /// <summary>
    ///     Identificador único.
    /// </summary>
    long Id,

    /// <summary>
    ///     Identificador do indexador (benchmark).
    /// </summary>
    int IndexadorId,

    /// <summary>
    ///     Nome do indexador.
    /// </summary>
    string? IndexadorNome,

    /// <summary>
    ///     Percentual do benchmark.
    /// </summary>
    decimal PercentualBenchmark,

    /// <summary>
    ///     Método de cálculo.
    /// </summary>
    MetodoCalculoPerformance MetodoCalculo,

    /// <summary>
    ///     Descrição do método de cálculo.
    /// </summary>
    string MetodoCalculoDescricao,

    /// <summary>
    ///     Indica se utiliza linha d'água.
    /// </summary>
    bool LinhaDagua,

    /// <summary>
    ///     Periodicidade de cristalização.
    /// </summary>
    PeriodicidadeCristalizacao PeriodicidadeCristalizacao,

    /// <summary>
    ///     Descrição da periodicidade de cristalização.
    /// </summary>
    string PeriodicidadeCristalizacaoDescricao,

    /// <summary>
    ///     Mês de cristalização.
    /// </summary>
    int? MesCristalizacao
);
