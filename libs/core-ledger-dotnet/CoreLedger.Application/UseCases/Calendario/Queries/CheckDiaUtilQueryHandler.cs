using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Handler for CheckDiaUtilQuery.
/// </summary>
public class CheckDiaUtilQueryHandler : IRequestHandler<CheckDiaUtilQuery, DiaUtilResultDto>
{
    private readonly ICalendarioQueryService _queryService;
    private readonly ILogger<CheckDiaUtilQueryHandler> _logger;

    public CheckDiaUtilQueryHandler(
        ICalendarioQueryService queryService,
        ILogger<CheckDiaUtilQueryHandler> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<DiaUtilResultDto> Handle(CheckDiaUtilQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Checking if {Data} is a business day for praca {Praca}",
            request.Data,
            request.Praca);

        var calendario = await _queryService.GetByDataAndPracaAsync(request.Data, request.Praca, cancellationToken);

        if (calendario == null)
        {
            // No calendar entry found - assume NOT a business day for safety
            return new DiaUtilResultDto(
                Data: request.Data,
                DiaUtil: false,
                TipoDia: Domain.Enums.TipoDia.FimDeSemana, // Default assumption
                Descricao: "Calendar entry not found");
        }

        return new DiaUtilResultDto(
            Data: calendario.Data,
            DiaUtil: calendario.DiaUtil,
            TipoDia: calendario.TipoDia,
            Descricao: calendario.Descricao);
    }
}
