using System.ComponentModel.DataAnnotations;

namespace CoreLedger.Application.DTOs.FundoPrazo;

/// <summary>
///     DTO para atualização de um Prazo Operacional de Fundo.
/// </summary>
public record FundoPrazoUpdateDto(
    /// <summary>
    ///     Dias para cotização (D+X).
    /// </summary>
    [Required(ErrorMessage = "Dias de cotização é obrigatório.")]
    [Range(0, 365, ErrorMessage = "Dias de cotização deve estar entre 0 e 365.")]
    int DiasCotizacao,

    /// <summary>
    ///     Dias para liquidação financeira (D+X).
    /// </summary>
    [Required(ErrorMessage = "Dias de liquidação é obrigatório.")]
    [Range(0, 365, ErrorMessage = "Dias de liquidação deve estar entre 0 e 365.")]
    int DiasLiquidacao,

    /// <summary>
    ///     Horário limite para solicitação (horário de corte).
    /// </summary>
    [Required(ErrorMessage = "Horário limite é obrigatório.")]
    TimeOnly HorarioLimite,

    /// <summary>
    ///     Indica se os dias são úteis (true) ou corridos (false).
    /// </summary>
    bool DiasUteis = true,

    /// <summary>
    ///     Dias de carência inicial.
    /// </summary>
    [Range(0, 3650, ErrorMessage = "Dias de carência deve estar entre 0 e 3650.")]
    int? DiasCarencia = null,

    /// <summary>
    ///     Identificador do calendário específico.
    /// </summary>
    int? CalendarioId = null,

    /// <summary>
    ///     Indica se permite operação parcial (resgate parcial).
    /// </summary>
    bool PermiteParcial = false,

    /// <summary>
    ///     Percentual mínimo para resgate parcial (%).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual mínimo deve estar entre 0 e 100.")]
    decimal? PercentualMinimo = null,

    /// <summary>
    ///     Valor mínimo para operação.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo não pode ser negativo.")]
    decimal? ValorMinimo = null
);
