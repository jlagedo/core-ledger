using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetTransactionByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetTransactionByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTransactionByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recuperando transação com ID: {TransactionId}", request.Id);

        var transaction = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType!)
                .ThenInclude(st => st.Type)
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction == null)
            throw new EntityNotFoundException("Transação", request.Id);

        return _mapper.Map<TransactionDto>(transaction);
    }
}