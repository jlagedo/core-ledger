using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Query to retrieve all Securities.
/// </summary>
public record GetAllSecuritiesQuery : IRequest<IReadOnlyList<SecurityDto>>;