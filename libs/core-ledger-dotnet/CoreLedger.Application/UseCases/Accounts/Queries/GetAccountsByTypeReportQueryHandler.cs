using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

public class
    GetAccountsByTypeReportQueryHandler : IRequestHandler<GetAccountsByTypeReportQuery,
    IReadOnlyList<AccountsByTypeReportDto>>
{
    private readonly IAccountQueryService _accountQueryService;
    private readonly ILogger<GetAccountsByTypeReportQueryHandler> _logger;

    public GetAccountsByTypeReportQueryHandler(
        IAccountQueryService accountQueryService,
        ILogger<GetAccountsByTypeReportQueryHandler> logger)
    {
        _accountQueryService = accountQueryService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountsByTypeReportDto>> Handle(
        GetAccountsByTypeReportQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando relatório de contas por tipo");

        var data = await _accountQueryService.GetActiveAccountsByTypeAsync(cancellationToken);

        var result = data.Select(d => new AccountsByTypeReportDto(
            d.TypeId,
            d.TypeDescription,
            d.ActiveAccountCount
        )).ToList();

        _logger.LogInformation("Relatório recuperado com {Count} tipos de conta", result.Count);

        return result;
    }
}