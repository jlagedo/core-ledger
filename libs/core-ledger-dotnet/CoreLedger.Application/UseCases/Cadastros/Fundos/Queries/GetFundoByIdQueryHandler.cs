using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Handler for GetFundoByIdQuery.
/// </summary>
public class GetFundoByIdQueryHandler : IRequestHandler<GetFundoByIdQuery, FundoResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFundoByIdQueryHandler> _logger;

    public GetFundoByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetFundoByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoResponseDto> Handle(GetFundoByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting fundo with ID {Id}", request.Id);

        var fundo = await _context.Fundos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id && f.DeletedAt == null, cancellationToken);

        if (fundo == null)
        {
            throw new EntityNotFoundException("Fundo", request.Id);
        }

        _logger.LogInformation("Fundo retrieved successfully - Id: {Id}, CNPJ: {Cnpj}, RazaoSocial: {RazaoSocial}", fundo.Id, fundo.Cnpj.Formatado, fundo.RazaoSocial);

        return _mapper.Map<FundoResponseDto>(fundo);
    }
}
