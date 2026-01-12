using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Vinculos.Queries;

/// <summary>
///     Handler for GetVinculosByFundoQuery.
/// </summary>
public class GetVinculosByFundoQueryHandler : IRequestHandler<GetVinculosByFundoQuery, IReadOnlyList<FundoVinculoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVinculosByFundoQueryHandler> _logger;

    public GetVinculosByFundoQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetVinculosByFundoQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FundoVinculoDto>> Handle(
        GetVinculosByFundoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting vínculos for fundo {FundoId}", request.FundoId);

        // Validate fundo exists
        var fundoExists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == request.FundoId && f.DeletedAt == null, cancellationToken);

        if (!fundoExists)
        {
            throw new EntityNotFoundException("Fundo", request.FundoId);
        }

        var query = _context.FundoVinculos
            .AsNoTracking()
            .Include(v => v.Instituicao)
            .Where(v => v.FundoId == request.FundoId);

        if (!request.IncluirEncerrados)
        {
            query = query.Where(v => v.DataFim == null);
        }

        var vinculos = await query
            .OrderBy(v => v.TipoVinculo)
            .ThenByDescending(v => v.Principal)
            .ThenBy(v => v.DataInicio)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} vínculos for fundo {FundoId} (IncluirEncerrados: {IncluirEncerrados})",
            vinculos.Count, request.FundoId, request.IncluirEncerrados);

        return vinculos.Select(v => _mapper.Map<FundoVinculoDto>(v)).ToList();
    }
}
