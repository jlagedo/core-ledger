using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Queries;

/// <summary>
///     Handler for retrieving all AccountType items.
/// </summary>
public class GetAllAccountTypesQueryHandler : IRequestHandler<GetAllAccountTypesQuery, IReadOnlyList<AccountTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllAccountTypesQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllAccountTypesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAllAccountTypesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountTypeDto>> Handle(
        GetAllAccountTypesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando todos os Tipos de Conta");

        var accountTypes = await _context.AccountTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = _mapper.Map<IReadOnlyList<AccountTypeDto>>(accountTypes);

        _logger.LogInformation("Recuperados {Count} Tipos de Conta", result.Count);

        return result;
    }
}
