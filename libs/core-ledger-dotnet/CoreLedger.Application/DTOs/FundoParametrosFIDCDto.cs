using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for FundoParametrosFIDC entity.
/// </summary>
public record FundoParametrosFIDCDto(
    long Id,
    Guid FundoId,
    TipoFIDC TipoFidc,
    string TipoFidcDescricao,
    List<TipoRecebiveis> TiposRecebiveis,
    List<string> TiposRecebiveisDescricao,
    int? PrazoMedioCarteira,
    decimal? IndiceSubordinacaoAlvo,
    decimal? IndiceSubordinacaoMinimo,
    decimal? ProvisaoDevedoresDuvidosos,
    decimal? LimiteConcentracaoCedente,
    decimal? LimiteConcentracaoSacado,
    bool PossuiCoobrigacao,
    decimal? PercentualCoobrigacao,
    Registradora? RegistradoraRecebiveis,
    string? RegistradoraRecebiveisDescricao,
    bool IntegracaoRegistradora,
    string? CodigoRegistradora,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO for creating new FIDC parameters.
/// </summary>
public record CreateFundoParametrosFIDCDto
{
    /// <summary>
    ///     Identificador do fundo (relacionamento 1:1).
    /// </summary>
    [Required]
    public Guid FundoId { get; init; }

    /// <summary>
    ///     Tipo do FIDC (Padronizado ou Não Padronizado).
    /// </summary>
    [Required]
    public TipoFIDC TipoFidc { get; init; }

    /// <summary>
    ///     Lista de tipos de recebíveis aceitos pelo FIDC.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Pelo menos um tipo de recebível deve ser informado.")]
    public List<TipoRecebiveis> TiposRecebiveis { get; init; } = new();

    /// <summary>
    ///     Prazo médio da carteira em dias.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Prazo médio da carteira deve ser maior que zero.")]
    public int? PrazoMedioCarteira { get; init; }

    /// <summary>
    ///     Índice de subordinação alvo (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Índice de subordinação alvo deve estar entre 0 e 1.")]
    public decimal? IndiceSubordinacaoAlvo { get; init; }

    /// <summary>
    ///     Índice de subordinação mínimo (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Índice de subordinação mínimo deve estar entre 0 e 1.")]
    public decimal? IndiceSubordinacaoMinimo { get; init; }

    /// <summary>
    ///     Provisão para devedores duvidosos (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Provisão para devedores duvidosos deve estar entre 0 e 1.")]
    public decimal? ProvisaoDevedoresDuvidosos { get; init; }

    /// <summary>
    ///     Limite de concentração por cedente (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Limite de concentração por cedente deve estar entre 0 e 1.")]
    public decimal? LimiteConcentracaoCedente { get; init; }

    /// <summary>
    ///     Limite de concentração por sacado (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Limite de concentração por sacado deve estar entre 0 e 1.")]
    public decimal? LimiteConcentracaoSacado { get; init; }

    /// <summary>
    ///     Indica se o fundo possui coobrigação.
    /// </summary>
    public bool PossuiCoobrigacao { get; init; }

    /// <summary>
    ///     Percentual de coobrigação (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Percentual de coobrigação deve estar entre 0 e 1.")]
    public decimal? PercentualCoobrigacao { get; init; }

    /// <summary>
    ///     Registradora de recebíveis utilizada.
    /// </summary>
    public Registradora? RegistradoraRecebiveis { get; init; }

    /// <summary>
    ///     Indica se possui integração com a registradora.
    /// </summary>
    public bool IntegracaoRegistradora { get; init; }

    /// <summary>
    ///     Código do fundo no sistema da registradora.
    /// </summary>
    [MaxLength(50)]
    public string? CodigoRegistradora { get; init; }
}

/// <summary>
///     DTO for updating existing FIDC parameters.
/// </summary>
public record UpdateFundoParametrosFIDCDto
{
    /// <summary>
    ///     Tipo do FIDC (Padronizado ou Não Padronizado).
    /// </summary>
    [Required]
    public TipoFIDC TipoFidc { get; init; }

    /// <summary>
    ///     Lista de tipos de recebíveis aceitos pelo FIDC.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Pelo menos um tipo de recebível deve ser informado.")]
    public List<TipoRecebiveis> TiposRecebiveis { get; init; } = new();

    /// <summary>
    ///     Prazo médio da carteira em dias.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Prazo médio da carteira deve ser maior que zero.")]
    public int? PrazoMedioCarteira { get; init; }

    /// <summary>
    ///     Índice de subordinação alvo (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Índice de subordinação alvo deve estar entre 0 e 1.")]
    public decimal? IndiceSubordinacaoAlvo { get; init; }

    /// <summary>
    ///     Índice de subordinação mínimo (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Índice de subordinação mínimo deve estar entre 0 e 1.")]
    public decimal? IndiceSubordinacaoMinimo { get; init; }

    /// <summary>
    ///     Provisão para devedores duvidosos (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Provisão para devedores duvidosos deve estar entre 0 e 1.")]
    public decimal? ProvisaoDevedoresDuvidosos { get; init; }

    /// <summary>
    ///     Limite de concentração por cedente (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Limite de concentração por cedente deve estar entre 0 e 1.")]
    public decimal? LimiteConcentracaoCedente { get; init; }

    /// <summary>
    ///     Limite de concentração por sacado (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Limite de concentração por sacado deve estar entre 0 e 1.")]
    public decimal? LimiteConcentracaoSacado { get; init; }

    /// <summary>
    ///     Indica se o fundo possui coobrigação.
    /// </summary>
    public bool PossuiCoobrigacao { get; init; }

    /// <summary>
    ///     Percentual de coobrigação (valor decimal entre 0 e 1, representando 0-100%).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Percentual de coobrigação deve estar entre 0 e 1.")]
    public decimal? PercentualCoobrigacao { get; init; }

    /// <summary>
    ///     Registradora de recebíveis utilizada.
    /// </summary>
    public Registradora? RegistradoraRecebiveis { get; init; }

    /// <summary>
    ///     Indica se possui integração com a registradora.
    /// </summary>
    public bool IntegracaoRegistradora { get; init; }

    /// <summary>
    ///     Código do fundo no sistema da registradora.
    /// </summary>
    [MaxLength(50)]
    public string? CodigoRegistradora { get; init; }
}
