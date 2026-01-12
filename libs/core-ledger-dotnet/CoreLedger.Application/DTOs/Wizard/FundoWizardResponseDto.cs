using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     DTO de resposta para criação de fundo via wizard.
/// </summary>
public record FundoWizardResponseDto(
    /// <summary>
    ///     Identificador único do fundo criado.
    /// </summary>
    Guid Id,

    /// <summary>
    ///     CNPJ do fundo (formatado).
    /// </summary>
    string Cnpj,

    /// <summary>
    ///     Razão social do fundo.
    /// </summary>
    string RazaoSocial,

    /// <summary>
    ///     Nome fantasia do fundo.
    /// </summary>
    string? NomeFantasia,

    /// <summary>
    ///     Tipo do fundo.
    /// </summary>
    TipoFundo TipoFundo,

    /// <summary>
    ///     Situação do fundo (sempre EmConstituicao para novos fundos).
    /// </summary>
    SituacaoFundo Situacao,

    /// <summary>
    ///     Data e hora de criação do fundo.
    /// </summary>
    DateTime CreatedAt
);
