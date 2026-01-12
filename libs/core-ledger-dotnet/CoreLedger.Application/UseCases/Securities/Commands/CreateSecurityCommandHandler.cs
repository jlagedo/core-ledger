using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Handler for creating a new Security.
/// </summary>
public class CreateSecurityCommandHandler : IRequestHandler<CreateSecurityCommand, SecurityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateSecurityCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateSecurityCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateSecurityCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SecurityDto> Handle(CreateSecurityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando segurança {Ticker} - Nome: {Name}, Isin: {Isin}, Tipo: {Type}, Moeda: {Currency}, CriadoPor: {UserId}",
            request.Ticker, request.Name, request.Isin, request.Type, request.Currency, request.CreatedByUserId);

        // Check if security with same ticker already exists
        var existing = await _context.Securities
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Ticker == request.Ticker, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Falha na criação de segurança: Ticker duplicado {Ticker} já existe como segurança {ExistingId}", request.Ticker, existing.Id);
            throw new DomainValidationException("Segurança com este ticker já existe");
        }

        var security = Security.Create(
            request.Name,
            request.Ticker,
            request.Isin,
            request.Type,
            request.Currency,
            request.CreatedByUserId);

        _context.Securities.Add(security);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Segurança criada com ID: {SecurityId}", security.Id);

        return _mapper.Map<SecurityDto>(security);
    }
}