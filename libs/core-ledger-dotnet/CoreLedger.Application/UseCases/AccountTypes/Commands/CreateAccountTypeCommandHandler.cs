using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Handler for creating a new AccountType.
/// </summary>
public class CreateAccountTypeCommandHandler : IRequestHandler<CreateAccountTypeCommand, AccountTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateAccountTypeCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateAccountTypeCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateAccountTypeCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountTypeDto> Handle(
        CreateAccountTypeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando novo tipo de conta com descrição: {Description}",
            request.Description);

        // Check if account type with same description already exists
        var existing = await _context.AccountTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Description.ToLower() == request.Description.ToLower(), cancellationToken);

        if (existing != null)
            throw new DomainValidationException("Tipo de conta com esta descrição já existe");

        var accountType = AccountType.Create(request.Description);

        _context.AccountTypes.Add(accountType);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tipo de conta criado com ID: {AccountTypeId}", accountType.Id);

        return _mapper.Map<AccountTypeDto>(accountType);
    }
}
