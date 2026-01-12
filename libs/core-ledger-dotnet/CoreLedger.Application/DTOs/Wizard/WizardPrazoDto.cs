using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Item de prazo operacional do fundo no wizard.
/// </summary>
public record WizardPrazoDto(
    /// <summary>
    ///     Tipo do prazo operacional (Aplicação, Resgate, Carência).
    /// </summary>
    [Required(ErrorMessage = "Tipo de operação é obrigatório.")]
    TipoPrazoOperacional TipoOperacao,

    /// <summary>
    ///     Dias para cotização (D+X).
    /// </summary>
    [Required(ErrorMessage = "Prazo de cotização é obrigatório.")]
    [Range(0, 365, ErrorMessage = "Prazo de cotização deve estar entre 0 e 365.")]
    int PrazoCotizacao,

    /// <summary>
    ///     Dias para liquidação financeira (D+X).
    /// </summary>
    [Required(ErrorMessage = "Prazo de liquidação é obrigatório.")]
    [Range(0, 365, ErrorMessage = "Prazo de liquidação deve estar entre 0 e 365.")]
    int PrazoLiquidacao,

    /// <summary>
    ///     Tipo de calendário (NACIONAL, ESTADOS_UNIDOS, etc.).
    /// </summary>
    [StringLength(20, ErrorMessage = "Tipo de calendário deve ter no máximo 20 caracteres.")]
    string TipoCalendario = "NACIONAL",

    /// <summary>
    ///     Valor mínimo para aplicação inicial.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo inicial não pode ser negativo.")]
    decimal? ValorMinimoInicial = null,

    /// <summary>
    ///     Valor mínimo para aplicação adicional.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo adicional não pode ser negativo.")]
    decimal? ValorMinimoAdicional = null,

    /// <summary>
    ///     Valor mínimo para resgate.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo de resgate não pode ser negativo.")]
    decimal? ValorMinimoResgate = null,

    /// <summary>
    ///     Valor mínimo de permanência.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo de permanência não pode ser negativo.")]
    decimal? ValorMinimoPermanencia = null,

    /// <summary>
    ///     Dias de carência inicial.
    /// </summary>
    [Range(0, 3650, ErrorMessage = "Prazo de carência deve estar entre 0 e 3650 dias.")]
    int? PrazoCarenciaDias = null,

    /// <summary>
    ///     Indica se permite resgate total.
    /// </summary>
    bool PermiteResgateTotal = true,

    /// <summary>
    ///     Indica se permite resgate programado.
    /// </summary>
    bool PermiteResgateProgramado = false,

    /// <summary>
    ///     Prazo máximo para programação de resgate (dias).
    /// </summary>
    [Range(0, 365, ErrorMessage = "Prazo máximo de programação deve estar entre 0 e 365.")]
    int? PrazoMaximoProgramacao = null,

    /// <summary>
    ///     Identificador da classe (opcional - prazo específico por classe).
    /// </summary>
    Guid? ClasseId = null
);
