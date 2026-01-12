namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
/// Método de cálculo da taxa de performance.
/// </summary>
public enum MetodoCalculoPerformance
{
    /// <summary>Método da cota ajustada.</summary>
    CotaAjustada = 1,

    /// <summary>Método High Water Mark (marca d'água).</summary>
    HighWaterMark = 2
}
