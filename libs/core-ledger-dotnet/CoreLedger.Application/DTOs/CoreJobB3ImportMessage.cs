namespace CoreLedger.Application.DTOs;

/// <summary>
///     Message DTO for B3 import job sent to RabbitMQ.
/// </summary>
public record CoreJobB3ImportMessage(
    int CoreJobId,
    string ReferenceId,
    string CommandType,
    string? CorrelationId = null
);