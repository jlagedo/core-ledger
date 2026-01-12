using ProtoBuf;

namespace CoreLedger.Application.Events;

/// <summary>
///     Domain event raised when a transaction is created.
///     Includes denormalized data for self-contained event processing.
///     Serialized using Protocol Buffers for compact binary representation.
/// </summary>
[ProtoContract]
public record TransactionCreatedEvent
{
    /// <summary>
    ///     Unique identifier of the created transaction.
    /// </summary>
    [ProtoMember(1)]
    public int TransactionId { get; init; }

    /// <summary>
    ///     Fund identifier.
    /// </summary>
    [ProtoMember(2)]
    public int FundId { get; init; }

    /// <summary>
    ///     Fund code (denormalized).
    /// </summary>
    [ProtoMember(3)]
    public string FundCode { get; set; } = null!;

    /// <summary>
    ///     Fund name (denormalized).
    /// </summary>
    [ProtoMember(4)]
    public string FundName { get; set; } = null!;

    /// <summary>
    ///     Security identifier (optional).
    /// </summary>
    [ProtoMember(5)]
    public int? SecurityId { get; init; }

    /// <summary>
    ///     Security ticker symbol (denormalized).
    /// </summary>
    [ProtoMember(6)]
    public string? SecurityTicker { get; init; }

    /// <summary>
    ///     Security name (denormalized).
    /// </summary>
    [ProtoMember(7)]
    public string? SecurityName { get; init; }

    /// <summary>
    ///     Transaction sub-type identifier.
    /// </summary>
    [ProtoMember(8)]
    public int TransactionSubTypeId { get; init; }

    /// <summary>
    ///     Transaction sub-type description (denormalized).
    /// </summary>
    [ProtoMember(9)]
    public string TransactionSubTypeDescription { get; set; } = null!;

    /// <summary>
    ///     Transaction type identifier (denormalized).
    /// </summary>
    [ProtoMember(10)]
    public int TransactionTypeId { get; init; }

    /// <summary>
    ///     Transaction type description (denormalized).
    /// </summary>
    [ProtoMember(11)]
    public string TransactionTypeDescription { get; set; } = null!;

    /// <summary>
    ///     Trade date.
    /// </summary>
    [ProtoMember(12)]
    public DateTime TradeDate { get; init; }

    /// <summary>
    ///     Settlement date.
    /// </summary>
    [ProtoMember(13)]
    public DateTime SettleDate { get; init; }

    /// <summary>
    ///     Transaction quantity.
    /// </summary>
    [ProtoMember(14)]
    public decimal Quantity { get; init; }

    /// <summary>
    ///     Transaction price per unit.
    /// </summary>
    [ProtoMember(15)]
    public decimal Price { get; init; }

    /// <summary>
    ///     Total transaction amount.
    /// </summary>
    [ProtoMember(16)]
    public decimal Amount { get; init; }

    /// <summary>
    ///     Currency code (ISO 4217 3-letter code).
    /// </summary>
    [ProtoMember(17)]
    public string Currency { get; set; } = null!;

    /// <summary>
    ///     Transaction status identifier.
    /// </summary>
    [ProtoMember(18)]
    public int StatusId { get; init; }

    /// <summary>
    ///     Transaction status description (denormalized).
    /// </summary>
    [ProtoMember(19)]
    public string StatusDescription { get; set; } = null!;

    /// <summary>
    ///     Timestamp when the transaction was created (UTC).
    /// </summary>
    [ProtoMember(20)]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    ///     User ID of the person who created the transaction.
    /// </summary>
    [ProtoMember(21)]
    public string CreatedByUserId { get; set; } = null!;

    /// <summary>
    ///     Correlation ID for distributed tracing.
    /// </summary>
    [ProtoMember(22)]
    public string? CorrelationId { get; init; }

    /// <summary>
    ///     Request ID for audit trail.
    /// </summary>
    [ProtoMember(23)]
    public string? RequestId { get; init; }

    /// <summary>
    ///     Timestamp when the event occurred (UTC).
    /// </summary>
    [ProtoMember(24)]
    public DateTime OccurredOn { get; init; }
}
