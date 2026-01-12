using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.AccountTypes.Queries;

/// <summary>
///     Query to retrieve a specific AccountType by ID.
/// </summary>
public record GetAccountTypeByIdQuery(int Id) : IRequest<AccountTypeDto>;