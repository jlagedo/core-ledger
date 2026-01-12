namespace CoreLedger.Application.DTOs;

/// <summary>
///     Response DTO for B3 instruction file import.
/// </summary>
public record ImportB3InstructionFileResponse(
    int CoreJobId,
    string ReferenceId,
    string Status,
    string Message
);