namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de fundo de investimento conforme classificação regulatória.
/// </summary>
public enum TipoFundo
{
    /// <summary>
    ///     Fundo de Investimento.
    /// </summary>
    FI = 1,

    /// <summary>
    ///     Fundo de Investimento em Cotas.
    /// </summary>
    FIC = 2,

    /// <summary>
    ///     Fundo de Investimento em Direitos Creditórios.
    /// </summary>
    FIDC = 3,

    /// <summary>
    ///     Fundo de Investimento em Participações.
    /// </summary>
    FIP = 4,

    /// <summary>
    ///     Fundo de Investimento Imobiliário.
    /// </summary>
    FII = 5,

    /// <summary>
    ///     Fundo de Investimento nas Cadeias Produtivas Agroindustriais.
    /// </summary>
    FIAGRO = 6,

    /// <summary>
    ///     Fundo de Investimento em Cotas de FIDC.
    /// </summary>
    FICFIDC = 7
}
