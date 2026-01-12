namespace CoreLedger.Application.DTOs;

/// <summary>
///     Lightweight DTO for fund autocomplete responses.
/// </summary>
public record FundAutocompleteDto(
    int Id,
    string Code,
    string Name
);
