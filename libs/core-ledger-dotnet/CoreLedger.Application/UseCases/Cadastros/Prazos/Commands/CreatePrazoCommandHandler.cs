using AutoMapper;
using CoreLedger.Application.DTOs.FundoPrazo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Prazos.Commands;

/// <summary>
///     Handler for CreatePrazoCommand.
/// </summary>
public class CreatePrazoCommandHandler : IRequestHandler<CreatePrazoCommand, FundoPrazoResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePrazoCommandHandler> _logger;

    public CreatePrazoCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreatePrazoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoPrazoResponseDto> Handle(CreatePrazoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando novo prazo {TipoPrazo} para o fundo {FundoId}",
            request.TipoPrazo,
            request.FundoId);

        // Validate fundo exists
        var fundoExists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (!fundoExists)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        // Validate classe if provided
        if (request.ClasseId.HasValue)
        {
            var classeExists = await _context.FundoClasses
                .AsNoTracking()
                .AnyAsync(c => c.Id == request.ClasseId.Value &&
                              c.FundoId == request.FundoId &&
                              c.DeletedAt == null, cancellationToken);

            if (!classeExists)
            {
                throw new EntityNotFoundException("Classe", request.ClasseId.Value);
            }
        }

        // Check for duplicate prazo type
        var existingPrazo = await _context.FundoPrazos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.FundoId == request.FundoId &&
                     p.TipoPrazo == request.TipoPrazo &&
                     p.ClasseId == request.ClasseId &&
                     p.Ativo,
                cancellationToken);

        if (existingPrazo != null)
        {
            throw new DomainValidationException(
                $"Já existe um prazo ativo do tipo {request.TipoPrazo} para este fundo/classe.");
        }

        // Create prazo
        var prazo = FundoPrazo.Criar(
            request.FundoId,
            request.TipoPrazo,
            request.DiasCotizacao,
            request.DiasLiquidacao,
            request.HorarioLimite,
            request.DiasUteis,
            request.ClasseId,
            request.DiasCarencia,
            request.CalendarioId,
            request.PermiteParcial,
            request.PercentualMinimo,
            request.ValorMinimo);

        _context.FundoPrazos.Add(prazo);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Prazo {Id} criado para o fundo {FundoId}",
            prazo.Id,
            request.FundoId);

        return _mapper.Map<FundoPrazoResponseDto>(prazo);
    }
}
