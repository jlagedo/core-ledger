namespace CoreLedger.Application.DTOs;

/// <summary>
///     Lightweight DTO for security autocomplete responses.
/// </summary>
public record SecurityAutocompleteDto(
    int Id,
    string Ticker,
    string Name
);
