using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

/// <summary>
///     Handler for retrieving transactions with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetTransactionsWithQueryQueryHandler
    : IRequestHandler<GetTransactionsWithQueryQuery, Application.Models.PagedResult<TransactionDto>>
{
    private readonly ITransactionQueryService _transactionQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionsWithQueryQueryHandler> _logger;

    public GetTransactionsWithQueryQueryHandler(
        ITransactionQueryService transactionQueryService,
        IMapper mapper,
        ILogger<GetTransactionsWithQueryQueryHandler> logger)
    {
        _transactionQueryService = transactionQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<TransactionDto>> Handle(
        GetTransactionsWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving transactions with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (transactions, totalCount) = await _transactionQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var transactionDtos = _mapper.Map<IReadOnlyList<TransactionDto>>(transactions);

        var result = new Application.Models.PagedResult<TransactionDto>(
            transactionDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);

        _logger.LogInformation(
            "Retrieved {Count} transactions out of {TotalCount} total",
            transactionDtos.Count, totalCount);

        return result;
    }
}
