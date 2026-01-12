namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Público-alvo do fundo de investimento conforme CVM 175.
/// </summary>
public enum PublicoAlvo
{
    /// <summary>
    ///     Investidores em geral (varejo).
    /// </summary>
    Geral = 1,

    /// <summary>
    ///     Investidores qualificados (patrimônio >= R$ 1 milhão).
    /// </summary>
    Qualificado = 2,

    /// <summary>
    ///     Investidores profissionais (patrimônio >= R$ 10 milhões).
    /// </summary>
    Profissional = 3
}
