namespace CoreLedger.Application.DTOs;

/// <summary>
///     Response DTO for test connection request.
/// </summary>
public record TestConnectionResponse(
    int CoreJobId,
    string ReferenceId,
    string Status,
    string Message,
    string? CorrelationId = null
);