using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;
using AppModels = CoreLedger.Application.Models;
using DomainModels = CoreLedger.Domain.Models;

namespace CoreLedger.Application.UseCases.Cadastros.Instituicoes.Queries;

/// <summary>
///     Handler for GetInstituicoesQuery.
/// </summary>
public class GetInstituicoesQueryHandler : IRequestHandler<GetInstituicoesQuery, AppModels.PagedResult<InstituicaoDto>>
{
    private readonly IInstituicaoQueryService _queryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInstituicoesQueryHandler> _logger;

    public GetInstituicoesQueryHandler(
        IInstituicaoQueryService queryService,
        IMapper mapper,
        ILogger<GetInstituicoesQueryHandler> logger)
    {
        _queryService = queryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AppModels.PagedResult<InstituicaoDto>> Handle(
        GetInstituicoesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting instituições with Limit={Limit}, Offset={Offset}, Search={Search}, Ativo={Ativo}",
            request.Limit, request.Offset, request.Search ?? "none", request.Ativo?.ToString() ?? "none");

        var parameters = new DomainModels.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var (instituicoes, totalCount) = await _queryService.GetWithQueryAsync(
            parameters,
            request.Search,
            request.Ativo,
            cancellationToken);

        var dtos = instituicoes.Select(i => _mapper.Map<InstituicaoDto>(i)).ToList();

        _logger.LogInformation("Retrieved {Count} instituições out of {TotalCount} total", dtos.Count, totalCount);

        return new AppModels.PagedResult<InstituicaoDto>(dtos, totalCount, request.Limit, request.Offset);
    }
}
