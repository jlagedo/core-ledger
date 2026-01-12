using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Query to retrieve a specific Security by ID.
/// </summary>
public record GetSecurityByIdQuery(int Id) : IRequest<SecurityDto>;