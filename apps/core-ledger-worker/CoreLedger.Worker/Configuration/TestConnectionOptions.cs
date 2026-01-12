namespace CoreLedger.Worker.Configuration;

/// <summary>
///     Test connection processing configuration options
/// </summary>
public class TestConnectionOptions
{
    /// <summary>
    ///     Simulated processing delay in milliseconds for testing
    /// </summary>
    public int SimulatedProcessingDelayMs { get; set; } = 2000;
}