namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de cálculo da cota do fundo de investimento.
/// </summary>
public enum TipoCota
{
    /// <summary>
    ///     Cota calculada no fechamento do dia (D+0).
    /// </summary>
    Fechamento = 1,

    /// <summary>
    ///     Cota de abertura (estimada).
    /// </summary>
    Abertura = 2
}
