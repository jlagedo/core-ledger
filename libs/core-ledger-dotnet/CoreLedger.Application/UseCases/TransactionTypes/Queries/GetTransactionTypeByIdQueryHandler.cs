using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public class GetTransactionTypeByIdQueryHandler : IRequestHandler<GetTransactionTypeByIdQuery, TransactionTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetTransactionTypeByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetTransactionTypeByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTransactionTypeByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionTypeDto> Handle(GetTransactionTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var type = await _context.TransactionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (type == null)
            throw new EntityNotFoundException("TransactionType", request.Id);

        return _mapper.Map<TransactionTypeDto>(type);
    }
}