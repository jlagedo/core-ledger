using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.AccountTypes.Queries;

/// <summary>
///     Query to retrieve all AccountType items.
/// </summary>
public record GetAllAccountTypesQuery : IRequest<IReadOnlyList<AccountTypeDto>>;