using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoPrazo;

/// <summary>
///     DTO resumido de um Prazo Operacional de Fundo para listagens.
/// </summary>
public record FundoPrazoListDto(
    /// <summary>
    ///     Identificador único do prazo.
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
    ///     Tipo do prazo operacional.
    /// </summary>
    TipoPrazoOperacional TipoPrazo,

    /// <summary>
    ///     Descrição do tipo do prazo.
    /// </summary>
    string TipoPrazoDescricao,

    /// <summary>
    ///     Dias para cotização (D+X).
    /// </summary>
    int DiasCotizacao,

    /// <summary>
    ///     Dias para liquidação (D+X).
    /// </summary>
    int DiasLiquidacao,

    /// <summary>
    ///     Horário limite.
    /// </summary>
    TimeOnly HorarioLimite,

    /// <summary>
    ///     Indica se o prazo está ativo.
    /// </summary>
    bool Ativo
);
