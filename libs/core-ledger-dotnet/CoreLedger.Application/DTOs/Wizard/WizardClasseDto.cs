using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Item de classe do fundo no wizard (para FIDC e fundos com múltiplas classes).
/// </summary>
public record WizardClasseDto(
    /// <summary>
    ///     Código identificador da classe (Ex: SR, MEZ, SUB).
    /// </summary>
    [Required(ErrorMessage = "Código da classe é obrigatório.")]
    [StringLength(20, ErrorMessage = "Código da classe deve ter no máximo 20 caracteres.")]
    string CodigoClasse,

    /// <summary>
    ///     Nome descritivo da classe.
    /// </summary>
    [Required(ErrorMessage = "Nome da classe é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome da classe deve ter no máximo 100 caracteres.")]
    string NomeClasse,

    /// <summary>
    ///     Público-alvo da classe.
    /// </summary>
    [Required(ErrorMessage = "Público-alvo da classe é obrigatório.")]
    PublicoAlvo PublicoAlvo,

    /// <summary>
    ///     Data de início da classe.
    /// </summary>
    [Required(ErrorMessage = "Data de início é obrigatória.")]
    DateOnly DataInicio,

    /// <summary>
    ///     CNPJ próprio da classe (se aplicável).
    /// </summary>
    [StringLength(18, ErrorMessage = "CNPJ da classe deve ter no máximo 18 caracteres.")]
    string? CnpjClasse = null,

    /// <summary>
    ///     Identificador da classe pai (para subclasses).
    /// </summary>
    Guid? ClassePaiId = null,

    /// <summary>
    ///     Nível hierárquico (1 = classe, 2 = subclasse).
    /// </summary>
    [Range(1, 2, ErrorMessage = "Nível deve ser 1 (classe) ou 2 (subclasse).")]
    int Nivel = 1,

    /// <summary>
    ///     Tipo da classe para FIDCs (Sênior, Mezanino, Subordinada).
    /// </summary>
    TipoClasseFIDC? TipoClasseFidc = null,

    /// <summary>
    ///     Ordem de subordinação para FIDCs (prioridade no recebimento).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Ordem de subordinação deve ser maior que zero.")]
    int? OrdemSubordinacao = null,

    /// <summary>
    ///     Rentabilidade alvo da classe (% a.a.).
    /// </summary>
    [Range(0, 1000, ErrorMessage = "Rentabilidade alvo deve estar entre 0 e 1000%.")]
    decimal? RentabilidadeAlvo = null,

    /// <summary>
    ///     Índice de subordinação mínimo (% entre 0 e 1).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Índice de subordinação mínimo deve estar entre 0 e 1.")]
    decimal? IndiceSubordinacaoMinimo = null,

    /// <summary>
    ///     Valor mínimo de aplicação na classe.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo de aplicação não pode ser negativo.")]
    decimal? ValorMinimoAplicacao = null,

    /// <summary>
    ///     Valor mínimo de permanência.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Valor mínimo de permanência não pode ser negativo.")]
    decimal? ValorMinimoPermanencia = null,

    /// <summary>
    ///     Indica se a classe possui responsabilidade limitada.
    /// </summary>
    bool ResponsabilidadeLimitada = true,

    /// <summary>
    ///     Indica se a classe possui segregação patrimonial.
    /// </summary>
    bool SegregacaoPatrimonial = true,

    /// <summary>
    ///     Taxa de administração específica da classe (% a.a.).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Taxa de administração deve estar entre 0 e 100.")]
    decimal? TaxaAdministracao = null,

    /// <summary>
    ///     Taxa de gestão específica da classe (% a.a.).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Taxa de gestão deve estar entre 0 e 100.")]
    decimal? TaxaGestao = null,

    /// <summary>
    ///     Taxa de performance específica da classe (% sobre o benchmark).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Taxa de performance deve estar entre 0 e 100.")]
    decimal? TaxaPerformance = null,

    /// <summary>
    ///     Identificador do benchmark para a classe.
    /// </summary>
    int? BenchmarkId = null,

    /// <summary>
    ///     Indica se permite resgate antecipado.
    /// </summary>
    bool PermiteResgateAntecipado = true,

    /// <summary>
    ///     Data de encerramento da classe.
    /// </summary>
    DateOnly? DataEncerramento = null
);
