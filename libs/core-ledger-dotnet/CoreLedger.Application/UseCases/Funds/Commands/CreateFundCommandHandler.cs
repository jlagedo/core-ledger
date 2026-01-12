using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy support

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
///     Handler for creating a new Fund.
/// </summary>
public class CreateFundCommandHandler : IRequestHandler<CreateFundCommand, FundDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateFundCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateFundCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateFundCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto> Handle(
        CreateFundCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando fundo {Code} - Nome: {Name}, Moeda: {BaseCurrency}, " +
            "DataInício: {InceptionDate}, FrequênciaAvaliação: {ValuationFrequency}, CriadoPor: {UserId}",
            request.Code, request.Name, request.BaseCurrency, request.InceptionDate, request.ValuationFrequency, request.CreatedByUserId);

        var existing = await _context.Funds
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Name == request.Name, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Falha na criação de fundo: Nome duplicado {FundName} já existe como fundo {ExistingId}", request.Name, existing.Id);
            throw new DomainValidationException("Fundo com este nome já existe");
        }

        var fund = Fund.Create(
            request.Code,
            request.Name,
            request.BaseCurrency,
            request.InceptionDate,
            request.ValuationFrequency,
            request.CreatedByUserId);

        _context.Funds.Add(fund);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fundo criado com ID: {FundId}", fund.Id);

        return _mapper.Map<FundDto>(fund);
    }
}