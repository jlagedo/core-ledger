using CoreLedger.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Handler for CheckCalendarioHealthQuery (CAL-002/CAL-003).
///     STUB: Returns placeholder result.
/// </summary>
public class CheckCalendarioHealthQueryHandler : IRequestHandler<CheckCalendarioHealthQuery, CalendarioHealthDto>
{
    private readonly ILogger<CheckCalendarioHealthQueryHandler> _logger;

    public CheckCalendarioHealthQueryHandler(ILogger<CheckCalendarioHealthQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<CalendarioHealthDto> Handle(CheckCalendarioHealthQuery request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("CheckCalendarioHealthQuery ainda não está implementada");

        // STUB: Return placeholder result
        var result = new CalendarioHealthDto(
            NacionalPreenchido: false,
            Proximo30DiasOk: false,
            Alertas: new List<string> { "Verificação de saúde ainda não implementada (CAL-002/CAL-003)" });

        return Task.FromResult(result);
    }
}
