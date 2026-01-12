using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public class
    GetAllTransactionSubTypesQueryHandler : IRequestHandler<GetAllTransactionSubTypesQuery,
    IReadOnlyList<TransactionSubTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllTransactionSubTypesQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllTransactionSubTypesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAllTransactionSubTypesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionSubTypeDto>> Handle(GetAllTransactionSubTypesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando subtipos de transação{TypeFilter}",
            request.TypeId.HasValue ? $" para tipo {request.TypeId}" : "");

        var query = _context.TransactionSubTypes.AsNoTracking();

        if (request.TypeId.HasValue)
            query = query.Where(s => s.TypeId == request.TypeId.Value);

        var subtypes = await query.ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<TransactionSubTypeDto>>(subtypes);
    }
}