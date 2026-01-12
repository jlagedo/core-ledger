using AutoMapper;
using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Commands;

/// <summary>
///     Handler for UpdateClasseCommand.
/// </summary>
public class UpdateClasseCommandHandler : IRequestHandler<UpdateClasseCommand, FundoClasseResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateClasseCommandHandler> _logger;

    public UpdateClasseCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UpdateClasseCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoClasseResponseDto> Handle(UpdateClasseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando classe com ID {Id}", request.Id);

        var classe = await _context.FundoClasses
            .Include(c => c.Fundo)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletedAt == null, cancellationToken);

        if (classe == null)
        {
            throw new EntityNotFoundException("Classe", request.Id);
        }

        // Update using domain method
        classe.Atualizar(
            request.NomeClasse,
            request.CnpjClasse,
            request.TipoClasseFidc,
            request.OrdemSubordinacao,
            request.RentabilidadeAlvo,
            request.ResponsabilidadeLimitada,
            request.SegregacaoPatrimonial,
            request.ValorMinimoAplicacao,
            classe.Fundo.TipoFundo);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Classe {Id} atualizada com sucesso", request.Id);

        return _mapper.Map<FundoClasseResponseDto>(classe);
    }
}
