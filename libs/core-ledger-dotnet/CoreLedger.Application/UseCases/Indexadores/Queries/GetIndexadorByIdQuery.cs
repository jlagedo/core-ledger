using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Queries;

/// <summary>
///     Query to get an Indexador by ID.
/// </summary>
public record GetIndexadorByIdQuery(int Id) : IRequest<IndexadorDto>;
