namespace CoreLedger.Worker.Configuration;

/// <summary>
/// Configuration options for RabbitMQ queue names used by the Worker.
/// </summary>
public class QueueNamesOptions
{
    /// <summary>
    /// Queue for transaction created events published from the outbox.
    /// </summary>
    public string TransactionCreated { get; set; } = "transaction.created.queue";

    /// <summary>
    /// Queue for B3 instruction file import jobs.
    /// </summary>
    public string B3Import { get; set; } = "worker.b3.import.queue";

    /// <summary>
    /// Queue for testing API to Worker connectivity.
    /// </summary>
    public string TestConnection { get; set; } = "worker.test.queue";
}
