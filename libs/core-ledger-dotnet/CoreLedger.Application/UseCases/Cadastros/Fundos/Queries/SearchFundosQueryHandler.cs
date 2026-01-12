using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Handler for SearchFundosQuery.
/// </summary>
public class SearchFundosQueryHandler : IRequestHandler<SearchFundosQuery, IReadOnlyList<FundoListDto>>
{
    private readonly IFundoQueryService _queryService;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchFundosQueryHandler> _logger;

    public SearchFundosQueryHandler(
        IFundoQueryService queryService,
        IMapper mapper,
        ILogger<SearchFundosQueryHandler> logger)
    {
        _queryService = queryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FundoListDto>> Handle(
        SearchFundosQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Searching fundos with term '{SearchTerm}' and limit {Limit}",
            request.SearchTerm, request.Limit);

        var fundos = await _queryService.SearchAsync(request.SearchTerm, request.Limit, cancellationToken);

        var dtos = fundos.Select(f => _mapper.Map<FundoListDto>(f)).ToList();

        _logger.LogInformation("Search completed - Found {Count} fundos for term '{SearchTerm}'", dtos.Count, request.SearchTerm);

        return dtos;
    }
}
