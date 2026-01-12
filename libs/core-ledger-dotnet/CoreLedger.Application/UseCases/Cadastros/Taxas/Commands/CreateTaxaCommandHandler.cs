using AutoMapper;
using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;

/// <summary>
///     Handler for CreateTaxaCommand.
/// </summary>
public class CreateTaxaCommandHandler : IRequestHandler<CreateTaxaCommand, FundoTaxaResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTaxaCommandHandler> _logger;

    public CreateTaxaCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateTaxaCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoTaxaResponseDto> Handle(CreateTaxaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando nova taxa {TipoTaxa} para o fundo {FundoId}",
            request.TipoTaxa,
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

        // Create taxa
        var taxa = FundoTaxa.Criar(
            request.FundoId,
            request.TipoTaxa,
            request.Percentual,
            request.BaseCalculo,
            request.PeriodicidadeProvisao,
            request.PeriodicidadePagamento,
            request.DataInicioVigencia,
            request.ClasseId,
            request.DiaPagamento,
            request.ValorMinimo,
            request.ValorMaximo);

        // Handle performance parameters
        if (request.TipoTaxa == TipoTaxa.Performance)
        {
            if (request.ParametrosPerformance == null)
            {
                throw new DomainValidationException(
                    "Parâmetros de performance são obrigatórios para taxa do tipo Performance.");
            }

            var parametros = FundoTaxaPerformance.CriarSemTaxa(
                request.ParametrosPerformance.IndexadorId,
                request.ParametrosPerformance.PercentualBenchmark,
                request.ParametrosPerformance.MetodoCalculo,
                request.ParametrosPerformance.LinhaDagua,
                request.ParametrosPerformance.PeriodicidadeCristalizacao,
                request.ParametrosPerformance.MesCristalizacao);

            taxa.DefinirParametrosPerformance(parametros);
        }

        _context.FundoTaxas.Add(taxa);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Taxa {Id} criada para o fundo {FundoId}",
            taxa.Id,
            request.FundoId);

        return _mapper.Map<FundoTaxaResponseDto>(taxa);
    }
}
