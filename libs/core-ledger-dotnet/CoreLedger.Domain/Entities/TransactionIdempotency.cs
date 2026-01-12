namespace CoreLedger.Domain.Entities;

/// <summary>
///     Represents an idempotency key entry for transaction deduplication.
///     Uses UUID v7 for the idempotency key to ensure temporal ordering and uniqueness.
/// </summary>
public class TransactionIdempotency
{
    /// <summary>
    ///     Private constructor for EF Core.
    /// </summary>
    private TransactionIdempotency()
    {
    }

    /// <summary>
    ///     Unique identifier for the idempotency entry.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     The idempotency key (UUID v7) provided by the client for transaction deduplication.
    ///     UUID v7 provides temporal ordering and uniqueness guarantees.
    /// </summary>
    public Guid IdempotencyKey { get; private set; }

    /// <summary>
    ///     Timestamp when the idempotency entry was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Identifier of the associated transaction (null if not yet processed).
    /// </summary>
    public int? TransactionId { get; private set; }

    /// <summary>
    ///     Factory method to create a new transaction idempotency entry.
    /// </summary>
    /// <param name="idempotencyKey">The UUID v7 idempotency key for deduplication.</param>
    /// <param name="transactionId">Optional transaction ID if already processed.</param>
    /// <returns>A new TransactionIdempotency instance.</returns>
    public static TransactionIdempotency Create(Guid idempotencyKey, int? transactionId = null)
    {
        if (idempotencyKey == Guid.Empty)
            throw new ArgumentException("Idempotency key cannot be empty.", nameof(idempotencyKey));

        return new TransactionIdempotency
        {
            IdempotencyKey = idempotencyKey,
            TransactionId = transactionId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Updates the associated transaction ID for a previously created idempotency entry.
    /// </summary>
    /// <param name="transactionId">The transaction ID to associate.</param>
    public void SetTransactionId(int transactionId)
    {
        TransactionId = transactionId;
    }
}
