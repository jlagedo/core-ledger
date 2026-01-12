namespace CoreLedger.Domain.Enums;

/// <summary>
///     Representa a localização do mercado (praça) para referência de calendário.
/// </summary>
public enum Praca
{
    /// <summary>
    ///     Calendário nacional (B3/ANBIMA).
    /// </summary>
    Nacional = 1,

    /// <summary>
    ///     Localização do mercado de São Paulo.
    /// </summary>
    SaoPaulo = 2,

    /// <summary>
    ///     Localização do mercado do Rio de Janeiro.
    /// </summary>
    RioDeJaneiro = 3,

    /// <summary>
    ///     Calendário do mercado americano (para ativos no exterior).
    /// </summary>
    ExteriorEua = 4,

    /// <summary>
    ///     Calendário do mercado europeu.
    /// </summary>
    ExteriorEur = 5
}
