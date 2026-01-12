using AutoMapper;
using CoreLedger.Application.DTOs.FundoPrazo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Prazos.Queries;

/// <summary>
///     Handler for GetPrazosByFundoQuery.
/// </summary>
public class GetPrazosByFundoQueryHandler : IRequestHandler<GetPrazosByFundoQuery, IReadOnlyList<FundoPrazoListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPrazosByFundoQueryHandler> _logger;

    public GetPrazosByFundoQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetPrazosByFundoQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FundoPrazoListDto>> Handle(
        GetPrazosByFundoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting prazos for fundo {FundoId}", request.FundoId);

        // Validate fundo exists
        var fundoExists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (!fundoExists)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        var query = _context.FundoPrazos
            .AsNoTracking()
            .Where(p => p.FundoId == request.FundoId);

        if (!request.IncluirInativos)
        {
            query = query.Where(p => p.Ativo);
        }

        var prazos = await query
            .OrderBy(p => p.TipoPrazo)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} prazos for fundo {FundoId} (IncluirInativos: {IncluirInativos})", prazos.Count, request.FundoId, request.IncluirInativos);

        return prazos.Select(p => _mapper.Map<FundoPrazoListDto>(p)).ToList();
    }
}
