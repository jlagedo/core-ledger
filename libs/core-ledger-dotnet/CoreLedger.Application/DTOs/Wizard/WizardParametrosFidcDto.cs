using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Parâmetros específicos de FIDC no wizard.
///     Obrigatório quando TipoFundo = FIDC ou FIDC_NP.
/// </summary>
public record WizardParametrosFidcDto(
    /// <summary>
    ///     Tipo do FIDC (Padronizado ou Não Padronizado).
    /// </summary>
    [Required(ErrorMessage = "Tipo do FIDC é obrigatório.")]
    TipoFIDC TipoFidc,

    /// <summary>
    ///     Lista de tipos de recebíveis aceitos pelo FIDC.
    /// </summary>
    [Required(ErrorMessage = "Tipos de recebíveis são obrigatórios.")]
    [MinLength(1, ErrorMessage = "Pelo menos um tipo de recebível deve ser informado.")]
    List<TipoRecebiveis> TiposRecebiveis,

    /// <summary>
    ///     Prazo médio da carteira em dias.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Prazo médio da carteira deve ser maior que zero.")]
    int? PrazoMedioCarteira = null,

    /// <summary>
    ///     Índice de subordinação alvo (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Índice de subordinação alvo deve estar entre 0 e 1.")]
    decimal? IndiceSubordinacaoAlvo = null,

    /// <summary>
    ///     Provisão para devedores duvidosos (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Provisão para devedores duvidosos deve estar entre 0 e 1.")]
    decimal? ProvisaoDevedoresDuvidosos = null,

    /// <summary>
    ///     Limite de concentração por cedente (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Limite de concentração por cedente deve estar entre 0 e 1.")]
    decimal? LimiteConcentracaoCedente = null,

    /// <summary>
    ///     Limite de concentração por sacado (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Limite de concentração por sacado deve estar entre 0 e 1.")]
    decimal? LimiteConcentracaoSacado = null,

    /// <summary>
    ///     Indica se o fundo possui coobrigação.
    /// </summary>
    bool PossuiCoobrigacao = false,

    /// <summary>
    ///     Percentual de coobrigação (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Percentual de coobrigação deve estar entre 0 e 1.")]
    decimal? PercentualCoobrigacao = null,

    /// <summary>
    ///     Indica se permite cessão parcial de recebíveis.
    /// </summary>
    bool PermiteCessaoParcial = true,

    /// <summary>
    ///     Rating mínimo dos recebíveis.
    /// </summary>
    [StringLength(10, ErrorMessage = "Rating mínimo deve ter no máximo 10 caracteres.")]
    string? RatingMinimo = null,

    /// <summary>
    ///     Agência de rating.
    /// </summary>
    [StringLength(50, ErrorMessage = "Agência de rating deve ter no máximo 50 caracteres.")]
    string? AgenciaRating = null,

    /// <summary>
    ///     Registradora de recebíveis utilizada.
    /// </summary>
    Registradora? RegistradoraRecebiveis = null,

    /// <summary>
    ///     Conta na registradora.
    /// </summary>
    [StringLength(50, ErrorMessage = "Conta na registradora deve ter no máximo 50 caracteres.")]
    string? ContaRegistradora = null,

    /// <summary>
    ///     Indica se possui integração com a registradora.
    /// </summary>
    bool IntegracaoRegistradora = false
);
