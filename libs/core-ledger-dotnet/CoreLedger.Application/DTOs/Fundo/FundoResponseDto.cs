using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Fundo;

/// <summary>
///     DTO de resposta completa de um Fundo.
/// </summary>
public record FundoResponseDto(
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
    ///     Nome fantasia do fundo.
    /// </summary>
    string? NomeFantasia,

    /// <summary>
    ///     Nome curto para exibição.
    /// </summary>
    string? NomeCurto,

    /// <summary>
    ///     Data de constituição do fundo.
    /// </summary>
    DateOnly? DataConstituicao,

    /// <summary>
    ///     Data de início das atividades do fundo.
    /// </summary>
    DateOnly? DataInicioAtividade,

    /// <summary>
    ///     Tipo do fundo.
    /// </summary>
    TipoFundo TipoFundo,

    /// <summary>
    ///     Descrição do tipo do fundo.
    /// </summary>
    string TipoFundoDescricao,

    /// <summary>
    ///     Classificação CVM do fundo.
    /// </summary>
    ClassificacaoCVM ClassificacaoCVM,

    /// <summary>
    ///     Descrição da classificação CVM.
    /// </summary>
    string ClassificacaoCVMDescricao,

    /// <summary>
    ///     Classificação ANBIMA do fundo.
    /// </summary>
    string? ClassificacaoAnbima,

    /// <summary>
    ///     Código ANBIMA do fundo (6 dígitos).
    /// </summary>
    string? CodigoAnbima,

    /// <summary>
    ///     Situação atual do fundo.
    /// </summary>
    SituacaoFundo Situacao,

    /// <summary>
    ///     Descrição da situação do fundo.
    /// </summary>
    string SituacaoDescricao,

    /// <summary>
    ///     Prazo do fundo.
    /// </summary>
    PrazoFundo Prazo,

    /// <summary>
    ///     Descrição do prazo do fundo.
    /// </summary>
    string PrazoDescricao,

    /// <summary>
    ///     Público-alvo do fundo.
    /// </summary>
    PublicoAlvo PublicoAlvo,

    /// <summary>
    ///     Descrição do público-alvo.
    /// </summary>
    string PublicoAlvoDescricao,

    /// <summary>
    ///     Regime de tributação do fundo.
    /// </summary>
    TributacaoFundo Tributacao,

    /// <summary>
    ///     Descrição do regime de tributação.
    /// </summary>
    string TributacaoDescricao,

    /// <summary>
    ///     Tipo de condomínio.
    /// </summary>
    TipoCondominio Condominio,

    /// <summary>
    ///     Descrição do tipo de condomínio.
    /// </summary>
    string CondominioDescricao,

    /// <summary>
    ///     Indica se o fundo é exclusivo.
    /// </summary>
    bool Exclusivo,

    /// <summary>
    ///     Indica se o fundo é reservado.
    /// </summary>
    bool Reservado,

    /// <summary>
    ///     Indica se o fundo permite alavancagem.
    /// </summary>
    bool PermiteAlavancagem,

    /// <summary>
    ///     Indica se o fundo aceita investimento em criptoativos.
    /// </summary>
    bool AceitaCripto,

    /// <summary>
    ///     Percentual máximo de investimento no exterior.
    /// </summary>
    decimal PercentualExterior,

    /// <summary>
    ///     Indica se o wizard de cadastro foi completado.
    /// </summary>
    bool WizardCompleto,

    /// <summary>
    ///     Progresso do cadastro (0-100%).
    /// </summary>
    int ProgressoCadastro,

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    DateTime CreatedAt,

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    DateTime? UpdatedAt
);
