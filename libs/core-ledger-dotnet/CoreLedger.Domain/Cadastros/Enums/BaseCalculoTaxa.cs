namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Base de cálculo para taxas do fundo de investimento.
/// </summary>
public enum BaseCalculoTaxa
{
    /// <summary>
    ///     Patrimônio líquido médio do período.
    /// </summary>
    PLMedio = 1,

    /// <summary>
    ///     Patrimônio líquido final do período.
    /// </summary>
    PLFinal = 2,

    /// <summary>
    ///     Total de cotas emitidas.
    /// </summary>
    CotasEmitidas = 3,

    /// <summary>
    ///     Valor da cota.
    /// </summary>
    Cota = 4,

    /// <summary>
    ///     Rentabilidade do período.
    /// </summary>
    Rentabilidade = 5
}
