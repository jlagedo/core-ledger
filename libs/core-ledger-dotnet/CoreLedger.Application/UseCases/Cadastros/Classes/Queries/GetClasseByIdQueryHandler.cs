using AutoMapper;
using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Queries;

/// <summary>
///     Handler for GetClasseByIdQuery.
/// </summary>
public class GetClasseByIdQueryHandler : IRequestHandler<GetClasseByIdQuery, FundoClasseResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetClasseByIdQueryHandler> _logger;

    public GetClasseByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetClasseByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundoClasseResponseDto> Handle(GetClasseByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting classe with ID {Id}", request.Id);

        var classe = await _context.FundoClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletedAt == null, cancellationToken);

        if (classe == null)
        {
            throw new EntityNotFoundException("Classe", request.Id);
        }

        _logger.LogInformation(
            "Classe retrieved successfully - Id: {Id}, CodigoClasse: {CodigoClasse}",
            classe.Id, classe.CodigoClasse);

        return _mapper.Map<FundoClasseResponseDto>(classe);
    }
}
