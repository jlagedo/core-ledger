using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Handler for CalcularDMaisQuery.
/// </summary>
public class CalcularDMaisQueryHandler : IRequestHandler<CalcularDMaisQuery, CalculoDMaisResultDto>
{
    private readonly ICalendarioQueryService _queryService;
    private readonly ILogger<CalcularDMaisQueryHandler> _logger;

    public CalcularDMaisQueryHandler(
        ICalendarioQueryService queryService,
        ILogger<CalcularDMaisQueryHandler> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<CalculoDMaisResultDto> Handle(CalcularDMaisQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Calculating D+{Dias} from {Data} for praca {Praca}",
            request.Dias,
            request.Data,
            request.Praca);

        var dataFinal = await _queryService.CalcularDMaisAsync(request.Data, request.Dias, request.Praca, cancellationToken);

        return new CalculoDMaisResultDto(
            DataInicial: request.Data,
            DiasUteis: request.Dias,
            DataFinal: dataFinal,
            Praca: request.Praca);
    }
}
