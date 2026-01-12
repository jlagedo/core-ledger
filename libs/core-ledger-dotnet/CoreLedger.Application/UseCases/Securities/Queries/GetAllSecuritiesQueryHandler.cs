using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Handler for retrieving all Securities.
/// </summary>
public class GetAllSecuritiesQueryHandler : IRequestHandler<GetAllSecuritiesQuery, IReadOnlyList<SecurityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllSecuritiesQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllSecuritiesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAllSecuritiesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SecurityDto>> Handle(
        GetAllSecuritiesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando todas as Segurança");

        var securities = await _context.Securities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var result = _mapper.Map<IReadOnlyList<SecurityDto>>(securities);

        _logger.LogInformation("Recuperadas {Count} Segurança", result.Count);

        return result;
    }
}