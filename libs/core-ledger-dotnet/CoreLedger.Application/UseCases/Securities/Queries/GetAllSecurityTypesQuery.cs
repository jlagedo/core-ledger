using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Query to retrieve all SecurityType enum values.
/// </summary>
public record GetAllSecurityTypesQuery : IRequest<IReadOnlyList<SecurityTypeDto>>;