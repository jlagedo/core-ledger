using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Command to create a new Indexador.
/// </summary>
public record CreateIndexadorCommand(
    string Codigo,
    string Nome,
    IndexadorTipo Tipo,
    string? Fonte,
    Periodicidade Periodicidade,
    decimal? FatorAcumulado,
    DateTime? DataBase,
    string? UrlFonte,
    bool ImportacaoAutomatica,
    bool Ativo
) : IRequest<IndexadorDto>;
