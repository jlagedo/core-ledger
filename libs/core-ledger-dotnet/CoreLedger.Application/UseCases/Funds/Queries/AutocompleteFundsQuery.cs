using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
///     Query to search funds for autocomplete using PostgreSQL full-text search.
/// </summary>
/// <param name="SearchTerm">The search term to match against fund code or name.</param>
public record AutocompleteFundsQuery(string? SearchTerm)
    : IRequest<IReadOnlyList<FundAutocompleteDto>>;
