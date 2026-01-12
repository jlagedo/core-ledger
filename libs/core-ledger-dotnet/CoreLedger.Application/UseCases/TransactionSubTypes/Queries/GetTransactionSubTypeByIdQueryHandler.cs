using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public class
    GetTransactionSubTypeByIdQueryHandler : IRequestHandler<GetTransactionSubTypeByIdQuery, TransactionSubTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetTransactionSubTypeByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetTransactionSubTypeByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTransactionSubTypeByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionSubTypeDto> Handle(GetTransactionSubTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var subtype = await _context.TransactionSubTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subtype == null)
            throw new EntityNotFoundException("TransactionSubType", request.Id);

        return _mapper.Map<TransactionSubTypeDto>(subtype);
    }
}