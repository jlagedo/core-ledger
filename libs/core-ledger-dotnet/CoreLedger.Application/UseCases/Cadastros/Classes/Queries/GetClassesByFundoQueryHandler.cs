using AutoMapper;
using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Queries;

/// <summary>
///     Handler for GetClassesByFundoQuery.
/// </summary>
public class GetClassesByFundoQueryHandler : IRequestHandler<GetClassesByFundoQuery, IReadOnlyList<FundoClasseListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetClassesByFundoQueryHandler> _logger;

    public GetClassesByFundoQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetClassesByFundoQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FundoClasseListDto>> Handle(
        GetClassesByFundoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting classes for fundo {FundoId}", request.FundoId);

        // Validate fundo exists
        var fundoExists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (!fundoExists)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        var classes = await _context.FundoClasses
            .AsNoTracking()
            .Where(c => c.FundoId == request.FundoId && c.DeletedAt == null)
            .OrderBy(c => c.CodigoClasse)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} classes for fundo {FundoId}", classes.Count, request.FundoId);

        return classes.Select(c => _mapper.Map<FundoClasseListDto>(c)).ToList();
    }
}
