using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
///     Handler for retrieving accounts with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetAccountsWithQueryQueryHandler
    : IRequestHandler<GetAccountsWithQueryQuery, Application.Models.PagedResult<AccountDto>>
{
    private readonly IAccountQueryService _accountQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAccountsWithQueryQueryHandler> _logger;

    public GetAccountsWithQueryQueryHandler(
        IAccountQueryService accountQueryService,
        IMapper mapper,
        ILogger<GetAccountsWithQueryQueryHandler> logger)
    {
        _accountQueryService = accountQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<AccountDto>> Handle(
        GetAccountsWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving accounts with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (accounts, totalCount) = await _accountQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var accountDtos = _mapper.Map<IReadOnlyList<AccountDto>>(accounts);

        var result = new Application.Models.PagedResult<AccountDto>(
            accountDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);

        _logger.LogInformation(
            "Retrieved {Count} accounts out of {TotalCount} total",
            accountDtos.Count, totalCount);

        return result;
    }
}
