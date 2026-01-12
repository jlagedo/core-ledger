using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Seção de classificação do fundo no wizard.
/// </summary>
public record WizardClassificacaoDto(
    /// <summary>
    ///     Classificação CVM do fundo.
    /// </summary>
    [Required(ErrorMessage = "Classificação CVM é obrigatória.")]
    ClassificacaoCVM ClassificacaoCvm,

    /// <summary>
    ///     Público-alvo do fundo.
    /// </summary>
    [Required(ErrorMessage = "Público-alvo é obrigatório.")]
    PublicoAlvo PublicoAlvo,

    /// <summary>
    ///     Regime de tributação do fundo.
    /// </summary>
    [Required(ErrorMessage = "Tributação é obrigatória.")]
    TributacaoFundo Tributacao,

    /// <summary>
    ///     Classificação ANBIMA do fundo.
    /// </summary>
    [StringLength(50, ErrorMessage = "Classificação ANBIMA deve ter no máximo 50 caracteres.")]
    string? ClassificacaoAnbima = null,

    /// <summary>
    ///     Código ANBIMA do fundo.
    /// </summary>
    [StringLength(20, ErrorMessage = "Código ANBIMA deve ter no máximo 20 caracteres.")]
    string? CodigoAnbima = null
);
