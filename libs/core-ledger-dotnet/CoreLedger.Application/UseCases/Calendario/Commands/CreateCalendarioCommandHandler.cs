using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Handler for CreateCalendarioCommand.
/// </summary>
public class CreateCalendarioCommandHandler : IRequestHandler<CreateCalendarioCommand, CalendarioDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCalendarioCommandHandler> _logger;

    public CreateCalendarioCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateCalendarioCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CalendarioDto> Handle(CreateCalendarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando nova entrada de Calendário para {Data} na praça {Praca}",
            request.Data,
            request.Praca);

        // CAL-001: Check for duplicate (data, praca) combination
        var existing = await _context.Calendarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Data == request.Data && c.Praca == request.Praca,
                cancellationToken);

        if (existing != null)
        {
            throw new DomainValidationException(
                $"Entrada de calendário para a data {request.Data:yyyy-MM-dd} e praça {request.Praca} já existe");
        }

        // Create new calendario entity using factory method
        var calendario = Domain.Entities.Calendario.Create(
            request.Data,
            request.TipoDia,
            request.Praca,
            request.Descricao,
            request.CreatedByUserId);

        _context.Calendarios.Add(calendario);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Entrada de calendário criada com ID {Id} para {Data}",
            calendario.Id,
            calendario.Data);

        return _mapper.Map<CalendarioDto>(calendario);
    }
}
