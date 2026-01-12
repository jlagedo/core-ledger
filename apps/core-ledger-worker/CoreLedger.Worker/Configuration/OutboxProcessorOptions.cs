namespace CoreLedger.Worker.Configuration;

/// <summary>
/// Configuration options for the TransactionOutboxProcessor background service.
/// Controls polling behavior, batch processing, and retry logic for the outbox pattern.
/// </summary>
public class OutboxProcessorOptions
{
    /// <summary>
    /// Polling interval in milliseconds - how often the processor checks for pending messages.
    /// Default: 1000ms (1 second).
    /// Lower values reduce latency but increase database load.
    /// </summary>
    public int PollingIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Maximum number of messages to fetch and process per batch.
    /// Default: 50.
    /// Higher values improve throughput but may cause longer processing delays.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Maximum number of retry attempts for failed messages.
    /// Default: 5 (total of 6 processing attempts including the initial attempt).
    /// After exceeding this count, messages remain in Failed status requiring manual intervention.
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;
}
