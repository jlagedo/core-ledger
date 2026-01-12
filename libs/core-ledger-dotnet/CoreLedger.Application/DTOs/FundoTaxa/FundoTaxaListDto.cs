using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoTaxa;

/// <summary>
///     DTO resumido de uma Taxa de Fundo para listagens.
/// </summary>
public record FundoTaxaListDto(
    /// <summary>
    ///     Identificador único da taxa.
    /// </summary>
    long Id,

    /// <summary>
    ///     Identificador do fundo.
    /// </summary>
    Guid FundoId,

    /// <summary>
    ///     Identificador da classe (se aplicável).
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
    ///     Data de início da vigência.
    /// </summary>
    DateOnly DataInicioVigencia,

    /// <summary>
    ///     Indica se a taxa está ativa.
    /// </summary>
    bool Ativa
);
