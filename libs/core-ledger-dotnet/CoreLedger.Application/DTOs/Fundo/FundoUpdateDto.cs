using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Fundo;

/// <summary>
///     DTO para atualização de dados cadastrais de um Fundo.
/// </summary>
public record FundoUpdateDto(
    /// <summary>
    ///     Razão social do fundo.
    /// </summary>
    [Required(ErrorMessage = "Razão social é obrigatória.")]
    [StringLength(200, ErrorMessage = "Razão social deve ter no máximo 200 caracteres.")]
    string RazaoSocial,

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
    ///     Classificação CVM do fundo.
    /// </summary>
    ClassificacaoCVM? ClassificacaoCVM = null,

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
    ///     Prazo do fundo (Determinado ou Indeterminado).
    /// </summary>
    PrazoFundo? Prazo = null,

    /// <summary>
    ///     Público-alvo do fundo.
    /// </summary>
    PublicoAlvo? PublicoAlvo = null,

    /// <summary>
    ///     Regime de tributação do fundo.
    /// </summary>
    TributacaoFundo? Tributacao = null,

    /// <summary>
    ///     Tipo de condomínio (Aberto ou Fechado).
    /// </summary>
    TipoCondominio? Condominio = null,

    /// <summary>
    ///     Indica se o fundo é exclusivo.
    /// </summary>
    bool? Exclusivo = null,

    /// <summary>
    ///     Indica se o fundo é reservado.
    /// </summary>
    bool? Reservado = null,

    /// <summary>
    ///     Indica se o fundo permite alavancagem.
    /// </summary>
    bool? PermiteAlavancagem = null,

    /// <summary>
    ///     Indica se o fundo aceita investimento em criptoativos.
    /// </summary>
    bool? AceitaCripto = null,

    /// <summary>
    ///     Percentual máximo de investimento no exterior (0-100).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual exterior deve estar entre 0 e 100.")]
    decimal? PercentualExterior = null
);
