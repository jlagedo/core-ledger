using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Item de vínculo institucional do fundo no wizard.
///     Diferente do CreateFundoVinculoDto, recebe CNPJ em vez de InstituicaoId.
///     O backend resolve o CNPJ para InstituicaoId.
/// </summary>
public record WizardVinculoDto(
    /// <summary>
    ///     Tipo de vínculo institucional.
    /// </summary>
    [Required(ErrorMessage = "Tipo de vínculo é obrigatório.")]
    TipoVinculoInstitucional TipoVinculo,

    /// <summary>
    ///     CNPJ da instituição (14 dígitos, com ou sem formatação).
    ///     Será resolvido para InstituicaoId no backend.
    /// </summary>
    [Required(ErrorMessage = "CNPJ da instituição é obrigatório.")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "CNPJ deve ter entre 14 e 18 caracteres.")]
    string CnpjInstituicao,

    /// <summary>
    ///     Nome da instituição (para exibição).
    /// </summary>
    [Required(ErrorMessage = "Nome da instituição é obrigatório.")]
    [StringLength(200, ErrorMessage = "Nome da instituição deve ter no máximo 200 caracteres.")]
    string NomeInstituicao,

    /// <summary>
    ///     Data de início do vínculo.
    /// </summary>
    [Required(ErrorMessage = "Data de início é obrigatória.")]
    DateOnly DataInicio,

    /// <summary>
    ///     Código CVM da instituição.
    /// </summary>
    [StringLength(20, ErrorMessage = "Código CVM deve ter no máximo 20 caracteres.")]
    string? CodigoCvm = null,

    /// <summary>
    ///     Data de fim do vínculo (se aplicável).
    /// </summary>
    DateOnly? DataFim = null,

    /// <summary>
    ///     Motivo do fim do vínculo.
    /// </summary>
    [StringLength(200, ErrorMessage = "Motivo do fim deve ter no máximo 200 caracteres.")]
    string? MotivoFim = null,

    /// <summary>
    ///     Nome do responsável na instituição.
    /// </summary>
    [StringLength(100, ErrorMessage = "Nome do responsável deve ter no máximo 100 caracteres.")]
    string? ResponsavelNome = null,

    /// <summary>
    ///     E-mail do responsável na instituição.
    /// </summary>
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "E-mail deve ter no máximo 100 caracteres.")]
    string? ResponsavelEmail = null,

    /// <summary>
    ///     Telefone do responsável na instituição.
    /// </summary>
    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres.")]
    string? ResponsavelTelefone = null
);
