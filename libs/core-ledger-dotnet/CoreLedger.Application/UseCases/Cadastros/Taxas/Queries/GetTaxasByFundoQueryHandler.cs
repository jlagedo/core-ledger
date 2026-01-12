using AutoMapper;
using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Queries;

/// <summary>
///     Handler for GetTaxasByFundoQuery.
/// </summary>
public class GetTaxasByFundoQueryHandler : IRequestHandler<GetTaxasByFundoQuery, IReadOnlyList<FundoTaxaListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaxasByFundoQueryHandler> _logger;

    public GetTaxasByFundoQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTaxasByFundoQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FundoTaxaListDto>> Handle(
        GetTaxasByFundoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting taxas for fundo {FundoId}", request.FundoId);

        // Validate fundo exists
        var fundoExists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (!fundoExists)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        var query = _context.FundoTaxas
            .AsNoTracking()
            .Include(t => t.ParametrosPerformance)
            .Where(t => t.FundoId == request.FundoId);

        if (!request.IncluirInativas)
        {
            query = query.Where(t => t.Ativa);
        }

        var taxas = await query
            .OrderBy(t => t.TipoTaxa)
            .ThenBy(t => t.DataInicioVigencia)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} taxas for fundo {FundoId} (IncluirInativas: {IncluirInativas})", taxas.Count, request.FundoId, request.IncluirInativas);

        return taxas.Select(t => _mapper.Map<FundoTaxaListDto>(t)).ToList();
    }
}
