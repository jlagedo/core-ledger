using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Handler for security autocomplete search using PostgreSQL full-text search.
/// </summary>
public class AutocompleteSecuritiesQueryHandler
    : IRequestHandler<AutocompleteSecuritiesQuery, IReadOnlyList<SecurityAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityQueryService _queryService;
    private readonly ILogger<AutocompleteSecuritiesQueryHandler> _logger;

    public AutocompleteSecuritiesQueryHandler(
        IApplicationDbContext context,
        ISecurityQueryService queryService,
        ILogger<AutocompleteSecuritiesQueryHandler> logger)
    {
        _context = context;
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SecurityAutocompleteDto>> Handle(
        AutocompleteSecuritiesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Searching securities for autocomplete with term: {SearchTerm}",
            request.SearchTerm ?? "<empty>");

        // If search term is empty or null, return recent securities
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var recentSecurities = await _context.Securities
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .Select(s => new SecurityAutocompleteDto(s.Id, s.Ticker, s.Name))
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Returned {Count} recent securities for empty search",
                recentSecurities.Count);

            return recentSecurities;
        }

        // Use query service for full-text search
        var securities = await _queryService.AutocompleteAsync(
            request.SearchTerm.Trim(),
            cancellationToken);

        var dtos = securities
            .Select(s => new SecurityAutocompleteDto(s.Id, s.Ticker, s.Name))
            .ToList();

        _logger.LogInformation(
            "Found {Count} securities matching autocomplete search '{SearchTerm}'",
            dtos.Count, request.SearchTerm);

        return dtos;
    }
}
