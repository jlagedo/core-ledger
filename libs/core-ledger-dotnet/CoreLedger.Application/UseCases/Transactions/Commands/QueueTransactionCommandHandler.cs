using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

/// <summary>
/// Handler for QueueTransactionCommand that publishes transaction events from the outbox to RabbitMQ.
/// Publishes raw Protobuf payloads to the transaction.created.queue for downstream consumers.
/// </summary>
public class QueueTransactionCommandHandler : IRequestHandler<QueueTransactionCommand>
{
    private readonly ILogger<QueueTransactionCommandHandler> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public QueueTransactionCommandHandler(
        ILogger<QueueTransactionCommandHandler> logger,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    /// <summary>
    /// Handles the queue transaction command by publishing the Protobuf payload to RabbitMQ.
    /// </summary>
    /// <param name="request">The command containing the Protobuf-serialized transaction event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task Handle(QueueTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Publicando evento de transação para RabbitMQ - IdMensagemOutbox: {OutboxMessageId}, " +
            "TamanhoCarga: {PayloadSize} bytes",
            request.OutboxMessageId,
            request.Payload.Length);

        // Publish raw Protobuf payload to RabbitMQ
        // Note: Correlation ID is embedded in the Protobuf payload and will be extracted by downstream consumers
        await _messagePublisher.PublishRawAsync(
            queueName: request.QueueName,
            payload: request.Payload,
            correlationId: null, // Embedded in Protobuf payload
            contentType: "application/protobuf",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Evento de transação publicado com sucesso em {QueueName} - IdMensagemOutbox: {OutboxMessageId}",
            request.QueueName,
            request.OutboxMessageId);
    }
}
