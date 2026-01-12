namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
/// Periodicidade de pagamento da taxa.
/// </summary>
public enum PeriodicidadePagamento
{
    /// <summary>Pagamento mensal.</summary>
    Mensal = 1,

    /// <summary>Pagamento trimestral.</summary>
    Trimestral = 2,

    /// <summary>Pagamento semestral.</summary>
    Semestral = 3,

    /// <summary>Pagamento anual.</summary>
    Anual = 4
}
