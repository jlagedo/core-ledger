using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
///     Handler for retrieving a specific Fund by ID.
/// </summary>
public class GetFundByIdQueryHandler : IRequestHandler<GetFundByIdQuery, FundDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetFundByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetFundByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetFundByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto> Handle(
        GetFundByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando Fundo com ID: {FundId}", request.Id);

        var fund = await _context.Funds
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (fund == null) throw new EntityNotFoundException("Fundo", request.Id);

        var result = _mapper.Map<FundDto>(fund);

        _logger.LogInformation("Fundo recuperado com ID: {FundId}", request.Id);

        return result;
    }
}