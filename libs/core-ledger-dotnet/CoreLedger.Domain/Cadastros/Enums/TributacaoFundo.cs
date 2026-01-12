namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Regime de tributação do fundo de investimento.
/// </summary>
public enum TributacaoFundo
{
    /// <summary>
    ///     Tributação de curto prazo (alíquota regressiva a partir de 22,5%).
    /// </summary>
    CurtoPrazo = 1,

    /// <summary>
    ///     Tributação de longo prazo (alíquota regressiva a partir de 22,5% até 15%).
    /// </summary>
    LongoPrazo = 2,

    /// <summary>
    ///     Tributação de fundos de ações (alíquota fixa de 15%).
    /// </summary>
    Acoes = 3,

    /// <summary>
    ///     Fundo isento de tributação.
    /// </summary>
    Isento = 4,

    /// <summary>
    ///     Regime de previdência (PGBL/VGBL).
    /// </summary>
    Previdencia = 5,

    /// <summary>
    ///     Tributação de fundos imobiliários (isenção PF em condições específicas).
    /// </summary>
    Imobiliario = 6
}
