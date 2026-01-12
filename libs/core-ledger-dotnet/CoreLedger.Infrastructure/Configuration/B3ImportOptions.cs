namespace CoreLedger.Infrastructure.Configuration;

/// <summary>
///     B3 import processing configuration options
/// </summary>
public class B3ImportOptions
{
    /// <summary>
    ///     Number of records to process in each batch
    /// </summary>
    public int BatchSize { get; set; } = 1000;
}