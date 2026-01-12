using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public class GetTransactionStatusByIdQueryHandler : IRequestHandler<GetTransactionStatusByIdQuery, TransactionStatusDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetTransactionStatusByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetTransactionStatusByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTransactionStatusByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionStatusDto> Handle(GetTransactionStatusByIdQuery request,
        CancellationToken cancellationToken)
    {
        var status = await _context.TransactionStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (status == null)
            throw new EntityNotFoundException("TransactionStatus", request.Id);

        return _mapper.Map<TransactionStatusDto>(status);
    }
}