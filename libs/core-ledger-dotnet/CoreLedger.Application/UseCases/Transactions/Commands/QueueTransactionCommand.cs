using MediatR;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

/// <summary>
/// Command to queue a transaction event for publishing to downstream systems.
/// This command receives a Protobuf-serialized TransactionCreatedEvent from the outbox processor
/// and handles publishing to RabbitMQ or other message brokers.
/// </summary>
/// <param name="Payload">The Protobuf-serialized transaction created event payload.</param>
/// <param name="OutboxMessageId">The outbox message ID for tracking and logging.</param>
/// <param name="QueueName">The name of the queue to publish to.</param>
public record QueueTransactionCommand(
    byte[] Payload,
    long OutboxMessageId,
    string QueueName
) : IRequest;
