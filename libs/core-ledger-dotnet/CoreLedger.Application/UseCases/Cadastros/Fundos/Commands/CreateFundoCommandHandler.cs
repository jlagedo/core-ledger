using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Handler for CreateFundoCommand.
/// </summary>
public class CreateFundoCommandHandler : IRequestHandler<CreateFundoCommand, FundoResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFundoCommandHandler> _logger;

    public CreateFundoCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateFundoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoResponseDto> Handle(CreateFundoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando novo fundo com CNPJ {Cnpj} e razão social {RazaoSocial}",
            request.Cnpj,
            request.RazaoSocial);

        // Create CNPJ value object for comparison (validates format early)
        var cnpjToCheck = CNPJ.Criar(request.Cnpj);

        // Check for duplicate CNPJ - EF Core uses value converter for the comparison
        var existingByCnpj = await _context.Fundos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                f => f.Cnpj == cnpjToCheck && f.DeletedAt == null,
                cancellationToken);

        if (existingByCnpj != null)
        {
            throw new DomainValidationException(
                $"Já existe um fundo cadastrado com o CNPJ {request.Cnpj}");
        }

        // Create new fundo using factory method
        var fundo = Fundo.Criar(
            request.Cnpj,
            request.RazaoSocial,
            request.TipoFundo,
            request.ClassificacaoCVM,
            request.Prazo,
            request.PublicoAlvo,
            request.Tributacao,
            request.Condominio,
            request.NomeFantasia,
            request.NomeCurto,
            request.DataConstituicao,
            request.DataInicioAtividade,
            request.ClassificacaoAnbima,
            request.CodigoAnbima,
            request.Exclusivo,
            request.Reservado,
            request.PermiteAlavancagem,
            request.AceitaCripto,
            request.PercentualExterior,
            request.CreatedBy);

        _context.Fundos.Add(fundo);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Fundo criado com ID {Id} para CNPJ {Cnpj}",
            fundo.Id,
            request.Cnpj);

        return _mapper.Map<FundoResponseDto>(fundo);
    }
}
