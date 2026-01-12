using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;

/// <summary>
///     Command to import historical data from CSV for an indexador.
/// </summary>
public record ImportHistoricoIndexadorCommand(
    int IndexadorId,
    byte[] CsvContent,
    bool Sobrescrever = false
) : IRequest<ImportHistoricoIndexadorResult>;

/// <summary>
///     Result of CSV import operation.
/// </summary>
public record ImportHistoricoIndexadorResult(
    int TotalRows,
    int ImportedRows,
    int SkippedRows,
    int OverwrittenRows,
    List<string> Errors
);
