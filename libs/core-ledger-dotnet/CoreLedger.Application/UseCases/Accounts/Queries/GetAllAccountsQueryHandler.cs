using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
///     Handler for retrieving all Account items.
/// </summary>
public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, IReadOnlyList<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllAccountsQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllAccountsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAllAccountsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountDto>> Handle(
        GetAllAccountsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando todas as Contas");

        var accounts = await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Type)
            .ToListAsync(cancellationToken);
        var result = _mapper.Map<IReadOnlyList<AccountDto>>(accounts);

        _logger.LogInformation("Recuperadas {Count} Contas", result.Count);

        return result;
    }
}