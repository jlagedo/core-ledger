using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Handler for retrieving a specific Security by ID.
/// </summary>
public class GetSecurityByIdQueryHandler : IRequestHandler<GetSecurityByIdQuery, SecurityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetSecurityByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetSecurityByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetSecurityByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SecurityDto> Handle(
        GetSecurityByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando Segurança com ID: {SecurityId}", request.Id);

        var security = await _context.Securities
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (security == null) throw new EntityNotFoundException("Segurança", request.Id);

        var result = _mapper.Map<SecurityDto>(security);

        _logger.LogInformation("Segurança recuperada com ID: {SecurityId}", request.Id);

        return result;
    }
}