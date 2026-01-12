namespace CoreLedger.Application.Constants;

/// <summary>
///     Centralized constants for RabbitMQ queue names used across the application.
/// </summary>
public static class QueueNames
{
    /// <summary>
    ///     Queue for B3 instruction file import jobs.
    /// </summary>
    public const string B3Import = "worker.b3.import.queue";

    /// <summary>
    ///     Queue for testing API to Worker connectivity.
    /// </summary>
    public const string TestConnection = "worker.test.queue";

    /// <summary>
    ///     Queue for transaction created events published from the outbox.
    /// </summary>
    public const string TransactionCreated = "transaction.created.queue";
}