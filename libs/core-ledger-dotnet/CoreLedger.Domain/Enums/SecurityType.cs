namespace CoreLedger.Domain.Enums;

/// <summary>
///     Representa o tipo de um valor mobiliário com base em padrões de contabilidade de fundos.
/// </summary>
public enum SecurityType
{
    Equity = 1, // Ação
    Bond = 2, // Renda Fixa
    Cash = 3,
    MoneyMarket = 4,
    MutualFund = 5,
    ETF = 6, // Fundo de Índice
    REIT = 7, // Fundo de Investimento Imobiliário
    Derivative = 8,
    Hybrid = 9,
    Future = 10,
    OptionOnEquity = 11,
    OptionOnFuture = 12,
    Forward = 13,
    Fund = 14,
    Receipt = 15,
    FX = 16, // Câmbio
    Commodity = 17,
    Index = 18
}