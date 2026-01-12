using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Fundo;

/// <summary>
///     DTO para criação de um novo Fundo.
/// </summary>
public record FundoCreateDto(
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
    [StringLength(200, ErrorMessage = "Razão social deve ter no máximo 200 caracteres.")]
    string RazaoSocial,

    /// <summary>
    ///     Tipo do fundo (FI, FIC, FIDC, etc.).
    /// </summary>
    [Required(ErrorMessage = "Tipo do fundo é obrigatório.")]
    TipoFundo TipoFundo,

    /// <summary>
    ///     Classificação CVM do fundo.
    /// </summary>
    [Required(ErrorMessage = "Classificação CVM é obrigatória.")]
    ClassificacaoCVM ClassificacaoCVM,

    /// <summary>
    ///     Prazo do fundo (Determinado ou Indeterminado).
    /// </summary>
    [Required(ErrorMessage = "Prazo é obrigatório.")]
    PrazoFundo Prazo,

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
    ///     Tipo de condomínio (Aberto ou Fechado).
    /// </summary>
    [Required(ErrorMessage = "Condomínio é obrigatório.")]
    TipoCondominio Condominio,

    /// <summary>
    ///     Nome fantasia do fundo.
    /// </summary>
    [StringLength(100, ErrorMessage = "Nome fantasia deve ter no máximo 100 caracteres.")]
    string? NomeFantasia = null,

    /// <summary>
    ///     Nome curto para exibição.
    /// </summary>
    [StringLength(30, ErrorMessage = "Nome curto deve ter no máximo 30 caracteres.")]
    string? NomeCurto = null,

    /// <summary>
    ///     Data de constituição do fundo.
    /// </summary>
    DateOnly? DataConstituicao = null,

    /// <summary>
    ///     Data de início das atividades do fundo.
    /// </summary>
    DateOnly? DataInicioAtividade = null,

    /// <summary>
    ///     Classificação ANBIMA do fundo.
    /// </summary>
    [StringLength(100, ErrorMessage = "Classificação ANBIMA deve ter no máximo 100 caracteres.")]
    string? ClassificacaoAnbima = null,

    /// <summary>
    ///     Código ANBIMA do fundo (6 dígitos).
    /// </summary>
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Código ANBIMA deve ter 6 dígitos.")]
    string? CodigoAnbima = null,

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
    ///     Indica se o fundo aceita investimento em criptoativos.
    /// </summary>
    bool AceitaCripto = false,

    /// <summary>
    ///     Percentual máximo de investimento no exterior (0-100).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual exterior deve estar entre 0 e 100.")]
    decimal PercentualExterior = 0
);
