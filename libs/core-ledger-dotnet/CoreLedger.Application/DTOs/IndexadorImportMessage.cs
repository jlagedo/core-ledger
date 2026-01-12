namespace CoreLedger.Application.DTOs;

/// <summary>
///     Message for triggering automatic indexador data import via RabbitMQ.
/// </summary>
public record IndexadorImportMessage(
    int IndexadorId,
    string UrlFonte,
    string CorrelationId
);
