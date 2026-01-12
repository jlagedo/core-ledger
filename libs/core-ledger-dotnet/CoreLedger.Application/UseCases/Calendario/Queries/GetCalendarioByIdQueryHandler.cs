using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Queries;

/// <summary>
///     Handler for GetCalendarioByIdQuery.
/// </summary>
public class GetCalendarioByIdQueryHandler : IRequestHandler<GetCalendarioByIdQuery, CalendarioDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCalendarioByIdQueryHandler> _logger;

    public GetCalendarioByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetCalendarioByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CalendarioDto> Handle(GetCalendarioByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo Calendário com ID {Id}", request.Id);

        var calendario = await _context.Calendarios
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (calendario == null)
        {
            throw new EntityNotFoundException("Calendário", request.Id);
        }

        return _mapper.Map<CalendarioDto>(calendario);
    }
}
