using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Handler for GetFundoByCnpjQuery.
/// </summary>
public class GetFundoByCnpjQueryHandler : IRequestHandler<GetFundoByCnpjQuery, FundoResponseDto>
{
    private readonly IFundoQueryService _queryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFundoByCnpjQueryHandler> _logger;

    public GetFundoByCnpjQueryHandler(
        IFundoQueryService queryService,
        IMapper mapper,
        ILogger<GetFundoByCnpjQueryHandler> logger)
    {
        _queryService = queryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoResponseDto> Handle(GetFundoByCnpjQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting fundo with CNPJ {Cnpj}", request.Cnpj);

        var fundo = await _queryService.GetByCnpjAsync(request.Cnpj, cancellationToken);

        if (fundo == null)
        {
            throw new EntityNotFoundException("Fundo", request.Cnpj);
        }

        _logger.LogInformation("Fundo retrieved by CNPJ - CNPJ: {Cnpj}, Id: {Id}, RazaoSocial: {RazaoSocial}", fundo.Cnpj.Formatado, fundo.Id, fundo.RazaoSocial);

        return _mapper.Map<FundoResponseDto>(fundo);
    }
}
