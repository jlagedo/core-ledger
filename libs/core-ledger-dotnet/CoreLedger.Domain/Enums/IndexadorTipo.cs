namespace CoreLedger.Domain.Enums;

/// <summary>
/// Tipos de indexadores econômicos
/// </summary>
public enum IndexadorTipo
{
    /// <summary>
    /// Taxas de juros (CDI, SELIC)
    /// </summary>
    Juros = 1,

    /// <summary>
    /// Índices de inflação (IPCA, IGP-M, INPC)
    /// </summary>
    Inflacao = 2,

    /// <summary>
    /// Taxas de câmbio (PTAX, DOLAR, EURO)
    /// </summary>
    Cambio = 3,

    /// <summary>
    /// Índices de bolsa (IBOVESPA, IBRX, SMLL)
    /// </summary>
    IndiceBolsa = 4,

    /// <summary>
    /// Índices de renda fixa (IMA-B, IMA-S)
    /// </summary>
    IndiceRendaFixa = 5,

    /// <summary>
    /// Índices de criptoativos
    /// </summary>
    Crypto = 6,

    /// <summary>
    /// Outros indicadores
    /// </summary>
    Outro = 7
}
