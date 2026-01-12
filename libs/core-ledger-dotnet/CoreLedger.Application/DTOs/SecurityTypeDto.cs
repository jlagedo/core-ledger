namespace CoreLedger.Application.DTOs;

/// <summary>
///     DTO for SecurityType enum values.
/// </summary>
public record SecurityTypeDto(
    int Value,
    string Name,
    string Description
);