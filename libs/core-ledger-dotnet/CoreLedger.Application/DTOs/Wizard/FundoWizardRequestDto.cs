using System.ComponentModel.DataAnnotations;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     DTO composto para criação de fundo via wizard.
///     Contém todas as seções do wizard em uma única requisição.
/// </summary>
public record FundoWizardRequestDto(
    /// <summary>
    ///     Seção de identificação do fundo (obrigatória).
    /// </summary>
    [Required(ErrorMessage = "Identificação é obrigatória.")]
    WizardIdentificacaoDto Identificacao,

    /// <summary>
    ///     Seção de classificação do fundo (obrigatória).
    /// </summary>
    [Required(ErrorMessage = "Classificação é obrigatória.")]
    WizardClassificacaoDto Classificacao,

    /// <summary>
    ///     Seção de características do fundo (obrigatória).
    /// </summary>
    [Required(ErrorMessage = "Características são obrigatórias.")]
    WizardCaracteristicasDto Caracteristicas,

    /// <summary>
    ///     Seção de parâmetros de cota (obrigatória).
    /// </summary>
    [Required(ErrorMessage = "Parâmetros de cota são obrigatórios.")]
    WizardParametrosCotaDto ParametrosCota,

    /// <summary>
    ///     Lista de taxas do fundo.
    ///     Obrigatória pelo menos uma taxa de administração.
    /// </summary>
    [Required(ErrorMessage = "Taxas são obrigatórias.")]
    [MinLength(1, ErrorMessage = "Pelo menos uma taxa deve ser informada.")]
    List<WizardTaxaDto> Taxas,

    /// <summary>
    ///     Lista de prazos operacionais do fundo.
    ///     Obrigatório prazo de aplicação e resgate.
    /// </summary>
    [Required(ErrorMessage = "Prazos são obrigatórios.")]
    [MinLength(1, ErrorMessage = "Pelo menos um prazo deve ser informado.")]
    List<WizardPrazoDto> Prazos,

    /// <summary>
    ///     Lista de vínculos institucionais do fundo.
    ///     Obrigatório: Administrador, Gestor, Custodiante.
    /// </summary>
    [Required(ErrorMessage = "Vínculos são obrigatórios.")]
    [MinLength(1, ErrorMessage = "Pelo menos um vínculo deve ser informado.")]
    List<WizardVinculoDto> Vinculos,

    /// <summary>
    ///     Lista de classes do fundo (para FIDC ou fundos com múltiplas classes).
    ///     Obrigatória para FIDC/FICFIDC.
    /// </summary>
    List<WizardClasseDto>? Classes = null,

    /// <summary>
    ///     Parâmetros específicos de FIDC.
    ///     Obrigatório quando TipoFundo = FIDC ou FICFIDC.
    /// </summary>
    WizardParametrosFidcDto? ParametrosFidc = null,

    /// <summary>
    ///     Identificador do rascunho (para cleanup após criação com sucesso).
    /// </summary>
    Guid? RascunhoId = null,

    /// <summary>
    ///     Lista de IDs de documentos temporários para vincular ao fundo.
    /// </summary>
    List<Guid>? DocumentosTempIds = null
);
