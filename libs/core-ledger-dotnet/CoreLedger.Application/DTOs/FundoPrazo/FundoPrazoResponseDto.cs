using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoPrazo;

/// <summary>
///     DTO de resposta completa de um Prazo Operacional de Fundo.
/// </summary>
public record FundoPrazoResponseDto(
    /// <summary>
    ///     Identificador único do prazo.
    /// </summary>
    long Id,

    /// <summary>
    ///     Identificador do fundo ao qual o prazo pertence.
    /// </summary>
    Guid FundoId,

    /// <summary>
    ///     Identificador da classe (se prazo específico por classe).
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
    ///     Dias para liquidação financeira (D+X).
    /// </summary>
    int DiasLiquidacao,

    /// <summary>
    ///     Dias de carência inicial.
    /// </summary>
    int? DiasCarencia,

    /// <summary>
    ///     Horário limite para solicitação.
    /// </summary>
    TimeOnly HorarioLimite,

    /// <summary>
    ///     Indica se os dias são úteis.
    /// </summary>
    bool DiasUteis,

    /// <summary>
    ///     Identificador do calendário específico.
    /// </summary>
    int? CalendarioId,

    /// <summary>
    ///     Indica se permite operação parcial.
    /// </summary>
    bool PermiteParcial,

    /// <summary>
    ///     Percentual mínimo para operação parcial.
    /// </summary>
    decimal? PercentualMinimo,

    /// <summary>
    ///     Valor mínimo para operação.
    /// </summary>
    decimal? ValorMinimo,

    /// <summary>
    ///     Indica se o prazo está ativo.
    /// </summary>
    bool Ativo,

    /// <summary>
    ///     Data de criação do registro.
    /// </summary>
    DateTime CreatedAt,

    /// <summary>
    ///     Data da última atualização do registro.
    /// </summary>
    DateTime? UpdatedAt
);
