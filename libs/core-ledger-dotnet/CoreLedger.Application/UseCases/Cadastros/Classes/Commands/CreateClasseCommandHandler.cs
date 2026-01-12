using AutoMapper;
using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Commands;

/// <summary>
///     Handler for CreateClasseCommand.
/// </summary>
public class CreateClasseCommandHandler : IRequestHandler<CreateClasseCommand, FundoClasseResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateClasseCommandHandler> _logger;

    public CreateClasseCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateClasseCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoClasseResponseDto> Handle(CreateClasseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando nova classe {CodigoClasse} para o fundo {FundoId}",
            request.CodigoClasse,
            request.FundoId);

        // Get the fundo to validate it exists and get its type
        var fundo = await _context.Fundos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (fundo == null)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        // Check for duplicate code
        var existingByCode = await _context.FundoClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.FundoId == request.FundoId &&
                     c.CodigoClasse == request.CodigoClasse.Trim().ToUpperInvariant() &&
                     c.DeletedAt == null,
                cancellationToken);

        if (existingByCode != null)
        {
            throw new DomainValidationException(
                $"Já existe uma classe com o código {request.CodigoClasse} para este fundo.");
        }

        // Create new classe using factory method
        var classe = FundoClasse.Criar(
            request.FundoId,
            request.CodigoClasse,
            request.NomeClasse,
            fundo.TipoFundo,
            request.CnpjClasse,
            request.TipoClasseFidc,
            request.OrdemSubordinacao,
            request.RentabilidadeAlvo,
            request.ResponsabilidadeLimitada,
            request.SegregacaoPatrimonial,
            request.ValorMinimoAplicacao);

        _context.FundoClasses.Add(classe);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Classe {Id} criada com código {CodigoClasse} para o fundo {FundoId}",
            classe.Id,
            request.CodigoClasse,
            request.FundoId);

        return _mapper.Map<FundoClasseResponseDto>(classe);
    }
}
