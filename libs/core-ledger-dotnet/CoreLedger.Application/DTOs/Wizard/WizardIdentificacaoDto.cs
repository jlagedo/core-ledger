using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Seção de identificação do fundo no wizard.
/// </summary>
public record WizardIdentificacaoDto(
    /// <summary>
    ///     CNPJ do fundo (14 dígitos, com ou sem formatação).
    /// </summary>
    [Required(ErrorMessage = "CNPJ é obrigatório.")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "CNPJ deve ter entre 14 e 18 caracteres.")]
    string Cnpj,

    /// <summary>
    ///     Razão social do fundo.
    /// </summary>
    [Required(ErrorMessage = "Razão social é obrigatória.")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Razão social deve ter entre 10 e 200 caracteres.")]
    string RazaoSocial,

    /// <summary>
    ///     Tipo do fundo (FI, FIC, FIDC, etc.).
    /// </summary>
    [Required(ErrorMessage = "Tipo do fundo é obrigatório.")]
    TipoFundo TipoFundo,

    /// <summary>
    ///     Data de constituição do fundo.
    /// </summary>
    [Required(ErrorMessage = "Data de constituição é obrigatória.")]
    DateOnly DataConstituicao,

    /// <summary>
    ///     Data de início das atividades do fundo.
    /// </summary>
    [Required(ErrorMessage = "Data de início de atividade é obrigatória.")]
    DateOnly DataInicioAtividade,

    /// <summary>
    ///     Nome fantasia do fundo.
    /// </summary>
    [StringLength(100, ErrorMessage = "Nome fantasia deve ter no máximo 100 caracteres.")]
    string? NomeFantasia = null,

    /// <summary>
    ///     Nome curto para exibição.
    /// </summary>
    [StringLength(50, ErrorMessage = "Nome curto deve ter no máximo 50 caracteres.")]
    string? NomeCurto = null
);
