using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.FundoClasse;

/// <summary>
///     DTO resumido de uma Classe de Fundo para listagens.
/// </summary>
public record FundoClasseListDto(
    /// <summary>
    ///     Identificador único da classe.
    /// </summary>
    Guid Id,

    /// <summary>
    ///     Identificador do fundo ao qual a classe pertence.
    /// </summary>
    Guid FundoId,

    /// <summary>
    ///     Código identificador da classe.
    /// </summary>
    string CodigoClasse,

    /// <summary>
    ///     Nome descritivo da classe.
    /// </summary>
    string NomeClasse,

    /// <summary>
    ///     Tipo da classe para FIDCs.
    /// </summary>
    TipoClasseFIDC? TipoClasseFidc,

    /// <summary>
    ///     Descrição do tipo de classe FIDC.
    /// </summary>
    string? TipoClasseFidcDescricao,

    /// <summary>
    ///     Indica se a classe está ativa.
    /// </summary>
    bool Ativa
);
