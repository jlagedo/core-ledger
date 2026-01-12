using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Command to update an existing Indexador.
///     Note: Tipo and Periodicidade are immutable after creation and cannot be changed.
/// </summary>
public record UpdateIndexadorCommand(
    int Id,
    string Nome,
    string? Fonte,
    decimal? FatorAcumulado,
    DateTime? DataBase,
    string? UrlFonte,
    bool ImportacaoAutomatica,
    bool Ativo
) : IRequest<IndexadorDto>;
