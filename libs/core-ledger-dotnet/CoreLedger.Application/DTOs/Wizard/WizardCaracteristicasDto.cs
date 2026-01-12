using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Seção de características do fundo no wizard.
/// </summary>
public record WizardCaracteristicasDto(
    /// <summary>
    ///     Tipo de condomínio (Aberto ou Fechado).
    /// </summary>
    [Required(ErrorMessage = "Condomínio é obrigatório.")]
    TipoCondominio Condominio,

    /// <summary>
    ///     Prazo do fundo (Determinado ou Indeterminado).
    /// </summary>
    [Required(ErrorMessage = "Prazo é obrigatório.")]
    PrazoFundo Prazo,

    /// <summary>
    ///     Data de encerramento (obrigatória para prazo determinado).
    /// </summary>
    DateOnly? DataEncerramento = null,

    /// <summary>
    ///     Indica se o fundo é exclusivo.
    /// </summary>
    bool Exclusivo = false,

    /// <summary>
    ///     Indica se o fundo é reservado.
    /// </summary>
    bool Reservado = false,

    /// <summary>
    ///     Indica se o fundo permite alavancagem.
    /// </summary>
    bool PermiteAlavancagem = false,

    /// <summary>
    ///     Limite de alavancagem (se permitido).
    /// </summary>
    [Range(0, 1000, ErrorMessage = "Limite de alavancagem deve estar entre 0 e 1000%.")]
    decimal? LimiteAlavancagem = null,

    /// <summary>
    ///     Indica se o fundo aceita investimento em criptoativos.
    /// </summary>
    bool AceitaCripto = false,

    /// <summary>
    ///     Percentual máximo de investimento no exterior (0-100).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual exterior deve estar entre 0 e 100.")]
    decimal PercentualExterior = 0
);
