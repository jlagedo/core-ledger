using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public class
    GetAllTransactionStatusesQueryHandler : IRequestHandler<GetAllTransactionStatusesQuery,
    IReadOnlyList<TransactionStatusDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllTransactionStatusesQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllTransactionStatusesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetAllTransactionStatusesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionStatusDto>> Handle(GetAllTransactionStatusesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando todos os status de transação");
        var statuses = await _context.TransactionStatuses
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TransactionStatusDto>>(statuses);
    }
}