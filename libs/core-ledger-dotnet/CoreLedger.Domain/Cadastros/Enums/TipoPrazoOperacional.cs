namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de prazo operacional do fundo de investimento.
/// </summary>
public enum TipoPrazoOperacional
{
    /// <summary>
    ///     Prazo para aplicação (cotização e liquidação).
    /// </summary>
    Aplicacao = 1,

    /// <summary>
    ///     Prazo para resgate (cotização e liquidação).
    /// </summary>
    Resgate = 2,

    /// <summary>
    ///     Prazo de carência para resgate.
    /// </summary>
    Carencia = 3
}
