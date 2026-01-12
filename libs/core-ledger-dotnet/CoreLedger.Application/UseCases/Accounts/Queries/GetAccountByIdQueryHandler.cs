using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
///     Handler for retrieving a specific Account by ID.
/// </summary>
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAccountByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAccountByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAccountByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando Conta com ID: {AccountId}", request.Id);

        var account = await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Type)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account == null) throw new EntityNotFoundException("Conta", request.Id);

        var result = _mapper.Map<AccountDto>(account);

        _logger.LogInformation("Conta recuperada com ID: {AccountId}", request.Id);

        return result;
    }
}