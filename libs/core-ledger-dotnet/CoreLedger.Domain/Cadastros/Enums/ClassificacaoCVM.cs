namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Classificação do fundo conforme CVM (Comissão de Valores Mobiliários).
/// </summary>
public enum ClassificacaoCVM
{
    /// <summary>
    ///     Fundo de Renda Fixa.
    /// </summary>
    RendaFixa = 1,

    /// <summary>
    ///     Fundo de Ações.
    /// </summary>
    Acoes = 2,

    /// <summary>
    ///     Fundo Cambial.
    /// </summary>
    Cambial = 3,

    /// <summary>
    ///     Fundo Multimercado.
    /// </summary>
    Multimercado = 4,

    /// <summary>
    ///     Fundo de Investimento em Direitos Creditórios.
    /// </summary>
    FIDC = 5,

    /// <summary>
    ///     Fundo de Investimento em Participações.
    /// </summary>
    FIP = 6,

    /// <summary>
    ///     Fundo de Investimento Imobiliário.
    /// </summary>
    FII = 7,

    /// <summary>
    ///     Fundo de Investimento nas Cadeias Produtivas Agroindustriais.
    /// </summary>
    FIAGRO = 8
}
