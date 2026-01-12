using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Instituicoes.Commands;

/// <summary>
///     Handler for CreateInstituicaoCommand.
/// </summary>
public class CreateInstituicaoCommandHandler : IRequestHandler<CreateInstituicaoCommand, InstituicaoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInstituicaoCommandHandler> _logger;

    public CreateInstituicaoCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateInstituicaoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InstituicaoDto> Handle(CreateInstituicaoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Criando nova instituição com CNPJ {Cnpj} e razão social {RazaoSocial}",
            request.Cnpj,
            request.RazaoSocial);

        // Create CNPJ value object for comparison (validates format early)
        var cnpjToCheck = CNPJ.Criar(request.Cnpj);

        // Check for duplicate CNPJ - EF Core uses value converter for the comparison
        var existingByCnpj = await _context.Instituicoes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                i => i.Cnpj == cnpjToCheck,
                cancellationToken);

        if (existingByCnpj != null)
        {
            throw new DomainValidationException(
                $"Já existe uma instituição cadastrada com o CNPJ {request.Cnpj}");
        }

        // Create new instituição using factory method
        var instituicao = Instituicao.Criar(
            request.Cnpj,
            request.RazaoSocial,
            request.NomeFantasia,
            request.Ativo);

        _context.Instituicoes.Add(instituicao);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Instituição criada com ID {Id} para CNPJ {Cnpj}",
            instituicao.Id,
            request.Cnpj);

             return _mapper.Map<InstituicaoDto>(instituicao);
    }
}
