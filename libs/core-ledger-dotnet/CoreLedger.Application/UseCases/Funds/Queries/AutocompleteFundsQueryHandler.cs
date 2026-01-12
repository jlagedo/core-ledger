using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
///     Handler for fund autocomplete search using PostgreSQL full-text search.
/// </summary>
public class AutocompleteFundsQueryHandler
    : IRequestHandler<AutocompleteFundsQuery, IReadOnlyList<FundAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFundQueryService _queryService;
    private readonly ILogger<AutocompleteFundsQueryHandler> _logger;

    public AutocompleteFundsQueryHandler(
        IApplicationDbContext context,
        IFundQueryService queryService,
        ILogger<AutocompleteFundsQueryHandler> logger)
    {
        _context = context;
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FundAutocompleteDto>> Handle(
        AutocompleteFundsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Searching funds for autocomplete with term: {SearchTerm}",
            request.SearchTerm ?? "<empty>");

        // If search term is empty or null, return recent funds
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var recentFunds = await _context.Funds
                .AsNoTracking()
                .OrderByDescending(f => f.CreatedAt)
                .Take(10)
                .Select(f => new FundAutocompleteDto(f.Id, f.Code, f.Name))
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Returned {Count} recent funds for empty search",
                recentFunds.Count);

            return recentFunds;
        }

        // Use query service for full-text search
        var funds = await _queryService.AutocompleteAsync(
            request.SearchTerm.Trim(),
            cancellationToken);

        var dtos = funds
            .Select(f => new FundAutocompleteDto(f.Id, f.Code, f.Name))
            .ToList();

        _logger.LogInformation(
            "Found {Count} funds matching autocomplete search '{SearchTerm}'",
            dtos.Count, request.SearchTerm);

        return dtos;
    }
}
