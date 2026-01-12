namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Situação operacional do fundo de investimento.
/// </summary>
public enum SituacaoFundo
{
    /// <summary>
    ///     Fundo em fase de constituição.
    /// </summary>
    EmConstituicao = 1,

    /// <summary>
    ///     Fundo ativo e operacional.
    /// </summary>
    Ativo = 2,

    /// <summary>
    ///     Fundo com operações suspensas temporariamente.
    /// </summary>
    Suspenso = 3,

    /// <summary>
    ///     Fundo em processo de liquidação.
    /// </summary>
    EmLiquidacao = 4,

    /// <summary>
    ///     Fundo liquidado e encerrado.
    /// </summary>
    Liquidado = 5
}
