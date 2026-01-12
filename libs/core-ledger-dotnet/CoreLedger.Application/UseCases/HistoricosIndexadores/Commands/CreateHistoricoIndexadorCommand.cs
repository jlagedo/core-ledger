using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;

/// <summary>
///     Command to create a new HistoricoIndexador record.
/// </summary>
public record CreateHistoricoIndexadorCommand(
    int IndexadorId,
    DateTime DataReferencia,
    decimal Valor,
    decimal? FatorDiario,
    decimal? VariacaoPercentual,
    string? Fonte,
    Guid? ImportacaoId
) : IRequest<HistoricoIndexadorDto>;
