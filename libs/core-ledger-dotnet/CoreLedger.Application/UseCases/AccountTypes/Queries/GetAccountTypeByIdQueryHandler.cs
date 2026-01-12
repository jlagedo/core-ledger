using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Queries;

/// <summary>
///     Handler for retrieving a specific AccountType by ID.
/// </summary>
public class GetAccountTypeByIdQueryHandler : IRequestHandler<GetAccountTypeByIdQuery, AccountTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAccountTypeByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAccountTypeByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAccountTypeByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountTypeDto> Handle(
        GetAccountTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando Tipo de Conta com ID: {AccountTypeId}", request.Id);

        var accountType = await _context.AccountTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Id == request.Id, cancellationToken);

        if (accountType == null)
            throw new EntityNotFoundException("Tipo de conta", request.Id);

        var result = _mapper.Map<AccountTypeDto>(accountType);

        _logger.LogInformation("Tipo de Conta recuperado com ID: {AccountTypeId}", request.Id);

        return result;
    }
}
