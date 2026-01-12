namespace CoreLedger.Infrastructure.Persistence.Models;

/// <summary>
///     Raw ETL data model for b3_instruments_enriched table.
///     This is NOT a domain entity - it represents external ETL process output.
/// </summary>
public class B3InstrumentsEnriched
{
    public string InstrumentName { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string? Isin { get; set; }
    public int SecurityTypeId { get; set; }
    public string Currency { get; set; } = string.Empty;
}