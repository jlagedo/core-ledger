namespace CoreLedger.Application.DTOs;

/// <summary>
///     Message DTO for testing API -> Queue -> Worker connection.
/// </summary>
public record TestConnectionMessage(
    int CoreJobId,
    string ReferenceId,
    string CommandType,
    string? CorrelationId = null
);