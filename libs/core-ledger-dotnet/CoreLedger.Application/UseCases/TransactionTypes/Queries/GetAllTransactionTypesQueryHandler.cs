using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public class
    GetAllTransactionTypesQueryHandler : IRequestHandler<GetAllTransactionTypesQuery, IReadOnlyList<TransactionTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllTransactionTypesQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllTransactionTypesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAllTransactionTypesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionTypeDto>> Handle(GetAllTransactionTypesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando todos os tipos de transação");
        var types = await _context.TransactionTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TransactionTypeDto>>(types);
    }
}