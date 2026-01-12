using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Query to search securities for autocomplete using PostgreSQL full-text search.
/// </summary>
/// <param name="SearchTerm">The search term to match against security ticker or name.</param>
public record AutocompleteSecuritiesQuery(string? SearchTerm)
    : IRequest<IReadOnlyList<SecurityAutocompleteDto>>;
