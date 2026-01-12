using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Handler for GetCalendariosWithQueryQuery.
/// </summary>
public class GetCalendariosWithQueryQueryHandler
    : IRequestHandler<GetCalendariosWithQueryQuery, PagedResult<CalendarioDto>>
{
    private readonly ICalendarioQueryService _queryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCalendariosWithQueryQueryHandler> _logger;

    public GetCalendariosWithQueryQueryHandler(
        ICalendarioQueryService queryService,
        IMapper mapper,
        ILogger<GetCalendariosWithQueryQueryHandler> logger)
    {
        _queryService = queryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<CalendarioDto>> Handle(
        GetCalendariosWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting calendarios with Limit={Limit}, Offset={Offset}, Search={Search}, Praca={Praca}, TipoDia={TipoDia}, DiaUtil={DiaUtil}, DataInicio={DataInicio}, DataFim={DataFim}",
            request.Limit, request.Offset, request.Search ?? "none",
            request.Praca?.ToString() ?? "none", request.TipoDia?.ToString() ?? "none",
            request.DiaUtil?.ToString() ?? "none", request.DataInicio ?? "none",
            request.DataFim ?? "none");

        // Parse date strings to DateOnly
        DateOnly? dataInicio = null;
        DateOnly? dataFim = null;

        if (!string.IsNullOrWhiteSpace(request.DataInicio) &&
            DateOnly.TryParse(request.DataInicio, out var parsedDataInicio))
        {
            dataInicio = parsedDataInicio;
        }

        if (!string.IsNullOrWhiteSpace(request.DataFim) &&
            DateOnly.TryParse(request.DataFim, out var parsedDataFim))
        {
            dataFim = parsedDataFim;
        }

        var parameters = new Domain.Models.CalendarioQueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search,
            Praca = request.Praca,
            TipoDia = request.TipoDia,
            DiaUtil = request.DiaUtil,
            DataInicio = dataInicio,
            DataFim = dataFim
        };

        var (calendarios, totalCount) = await _queryService.GetWithQueryAsync(parameters, cancellationToken);

        var dtos = calendarios.Select(c => _mapper.Map<CalendarioDto>(c)).ToList();

        return new PagedResult<CalendarioDto>(dtos, totalCount, request.Limit, request.Offset);
    }
}
