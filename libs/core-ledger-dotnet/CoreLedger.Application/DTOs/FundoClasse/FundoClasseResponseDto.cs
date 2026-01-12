using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoClasse;

/// <summary>
///     DTO de resposta completa de uma Classe de Fundo.
/// </summary>
public record FundoClasseResponseDto(
    /// <summary>
    ///     Identificador único da classe.
    /// </summary>
    Guid Id,

    /// <summary>
    ///     Identificador do fundo ao qual a classe pertence.
    /// </summary>
    Guid FundoId,

    /// <summary>
    ///     Código identificador da classe (Ex: SR, MEZ, SUB).
    /// </summary>
    string CodigoClasse,

    /// <summary>
    ///     Nome descritivo da classe.
    /// </summary>
    string NomeClasse,

    /// <summary>
    ///     CNPJ próprio da classe (se aplicável).
    /// </summary>
    string? CnpjClasse,

    /// <summary>
    ///     Tipo da classe para FIDCs.
    /// </summary>
    TipoClasseFIDC? TipoClasseFidc,

    /// <summary>
    ///     Descrição do tipo de classe FIDC.
    /// </summary>
    string? TipoClasseFidcDescricao,

    /// <summary>
    ///     Ordem de subordinação para FIDCs.
    /// </summary>
    int? OrdemSubordinacao,

    /// <summary>
    ///     Rentabilidade alvo da classe (% a.a.).
    /// </summary>
    decimal? RentabilidadeAlvo,

    /// <summary>
    ///     Indica se a classe possui responsabilidade limitada.
    /// </summary>
    bool ResponsabilidadeLimitada,

    /// <summary>
    ///     Indica se a classe possui segregação patrimonial.
    /// </summary>
    bool SegregacaoPatrimonial,

    /// <summary>
    ///     Valor mínimo de aplicação na classe.
    /// </summary>
    decimal? ValorMinimoAplicacao,

    /// <summary>
    ///     Indica se a classe está ativa.
    /// </summary>
    bool Ativa,

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    DateTime CreatedAt,

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    DateTime? UpdatedAt
);
