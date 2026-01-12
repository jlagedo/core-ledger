namespace CoreLedger.Application.DTOs;

/// <summary>
///     Request DTO for testing API -> Queue -> Worker connection.
/// </summary>
public record TestConnectionRequest(
    string ReferenceId,
    string? JobDescription = null
);