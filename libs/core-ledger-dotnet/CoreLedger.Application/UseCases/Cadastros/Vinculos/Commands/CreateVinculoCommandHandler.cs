using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Vinculos.Commands;

/// <summary>
///     Handler for CreateVinculoCommand.
/// </summary>
public class CreateVinculoCommandHandler : IRequestHandler<CreateVinculoCommand, FundoVinculoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateVinculoCommandHandler> _logger;

    public CreateVinculoCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateVinculoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoVinculoDto> Handle(CreateVinculoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando novo vínculo {TipoVinculo} para o fundo {FundoId} com instituição {InstituicaoId}",
            request.TipoVinculo,
            request.FundoId,
            request.InstituicaoId);

        // Validate fundo exists
        var fundoExists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (!fundoExists)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        // Validate instituicao exists
        var instituicaoExists = await _context.Instituicoes
            .AsNoTracking()
            .AnyAsync(i => i.Id == request.InstituicaoId && i.Ativo, cancellationToken);

        if (!instituicaoExists)
        {
            throw new EntityNotFoundException("Instituição", request.InstituicaoId);
        }

        // Check for duplicate active vínculo of same type
        var existingVinculo = await _context.FundoVinculos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                v => v.FundoId == request.FundoId &&
                     v.InstituicaoId == request.InstituicaoId &&
                     v.TipoVinculo == request.TipoVinculo &&
                     v.DataFim == null,
                cancellationToken);

        if (existingVinculo != null)
        {
            throw new DomainValidationException(
                $"Já existe um vínculo ativo do tipo {request.TipoVinculo} entre este fundo e instituição.");
        }

        // If setting as principal, unset other principals of same type
        if (request.Principal)
        {
            var otherPrincipals = await _context.FundoVinculos
                .Where(v => v.FundoId == request.FundoId &&
                           v.TipoVinculo == request.TipoVinculo &&
                           v.Principal &&
                           v.DataFim == null)
                .ToListAsync(cancellationToken);

            foreach (var vp in otherPrincipals)
            {
                vp.DefinirComoPrincipal(false);
            }
        }

        // Create vínculo
        var vinculo = FundoVinculo.Criar(
            request.FundoId,
            request.InstituicaoId,
            request.TipoVinculo,
            request.DataInicio,
            request.Principal,
            request.ContratoNumero,
            request.Observacao);

        _context.FundoVinculos.Add(vinculo);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with related entities
        vinculo = await _context.FundoVinculos
            .AsNoTracking()
            .Include(v => v.Instituicao)
            .FirstAsync(v => v.Id == vinculo.Id, cancellationToken);

        _logger.LogInformation(
            "Vínculo {Id} criado para o fundo {FundoId}",
            vinculo.Id,
            request.FundoId);

        return _mapper.Map<FundoVinculoDto>(vinculo);
    }
}
