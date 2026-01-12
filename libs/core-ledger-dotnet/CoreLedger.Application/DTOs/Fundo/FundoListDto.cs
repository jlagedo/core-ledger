using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Fundo;

/// <summary>
///     DTO resumido para listagem de Fundos.
/// </summary>
public record FundoListDto(
    /// <summary>
    ///     Identificador único do fundo.
    /// </summary>
    Guid Id,

    /// <summary>
    ///     CNPJ do fundo (14 dígitos, sem formatação).
    /// </summary>
    string Cnpj,

    /// <summary>
    ///     CNPJ do fundo formatado (XX.XXX.XXX/XXXX-XX).
    /// </summary>
    string CnpjFormatado,

    /// <summary>
    ///     Razão social do fundo.
    /// </summary>
    string RazaoSocial,

    /// <summary>
    ///     Nome curto para exibição.
    /// </summary>
    string? NomeCurto,

    /// <summary>
    ///     Tipo do fundo.
    /// </summary>
    TipoFundo TipoFundo,

    /// <summary>
    ///     Descrição do tipo do fundo.
    /// </summary>
    string TipoFundoDescricao,

    /// <summary>
    ///     Situação atual do fundo.
    /// </summary>
    SituacaoFundo Situacao,

    /// <summary>
    ///     Descrição da situação do fundo.
    /// </summary>
    string SituacaoDescricao,

    /// <summary>
    ///     Progresso do cadastro (0-100%).
    /// </summary>
    int ProgressoCadastro
);
