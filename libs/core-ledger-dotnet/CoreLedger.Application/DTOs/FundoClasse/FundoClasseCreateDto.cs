using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoClasse;

/// <summary>
///     DTO para criação de uma nova Classe de Fundo.
/// </summary>
public record FundoClasseCreateDto(
    /// <summary>
    ///     Código identificador da classe (Ex: SR, MEZ, SUB).
    /// </summary>
    [Required(ErrorMessage = "Código da classe é obrigatório.")]
    [StringLength(10, ErrorMessage = "Código da classe deve ter no máximo 10 caracteres.")]
    string CodigoClasse,

    /// <summary>
    ///     Nome descritivo da classe.
    /// </summary>
    [Required(ErrorMessage = "Nome da classe é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome da classe deve ter no máximo 100 caracteres.")]
    string NomeClasse,

    /// <summary>
    ///     CNPJ próprio da classe (se aplicável).
    /// </summary>
    [StringLength(14, ErrorMessage = "CNPJ da classe deve ter no máximo 14 caracteres.")]
    string? CnpjClasse = null,

    /// <summary>
    ///     Tipo da classe para FIDCs (Sênior, Mezanino, Subordinada).
    ///     Obrigatório para fundos FIDC.
    /// </summary>
    TipoClasseFIDC? TipoClasseFidc = null,

    /// <summary>
    ///     Ordem de subordinação para FIDCs (prioridade no recebimento).
    ///     Obrigatório para fundos FIDC.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Ordem de subordinação deve ser maior que zero.")]
    int? OrdemSubordinacao = null,

    /// <summary>
    ///     Rentabilidade alvo da classe (% a.a.).
    /// </summary>
    [Range(0, 1000, ErrorMessage = "Rentabilidade alvo deve estar entre 0 e 1000%.")]
    decimal? RentabilidadeAlvo = null,

    /// <summary>
    ///     Indica se a classe possui responsabilidade limitada.
    /// </summary>
    bool ResponsabilidadeLimitada = false,

    /// <summary>
    ///     Indica se a classe possui segregação patrimonial.
    /// </summary>
    bool SegregacaoPatrimonial = false,

    /// <summary>
    ///     Valor mínimo de aplicação na classe.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo de aplicação não pode ser negativo.")]
    decimal? ValorMinimoAplicacao = null
);
