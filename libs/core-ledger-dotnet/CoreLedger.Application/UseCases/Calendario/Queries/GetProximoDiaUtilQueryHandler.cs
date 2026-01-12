using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Handler for GetProximoDiaUtilQuery.
/// </summary>
public class GetProximoDiaUtilQueryHandler : IRequestHandler<GetProximoDiaUtilQuery, DateOnly>
{
    private readonly ICalendarioQueryService _queryService;
    private readonly ILogger<GetProximoDiaUtilQueryHandler> _logger;

    public GetProximoDiaUtilQueryHandler(
        ICalendarioQueryService queryService,
        ILogger<GetProximoDiaUtilQueryHandler> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<DateOnly> Handle(GetProximoDiaUtilQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting next business day after {Data} for praca {Praca}",
            request.Data,
            request.Praca);

        return await _queryService.GetProximoDiaUtilAsync(request.Data, request.Praca, cancellationToken);
    }
}
