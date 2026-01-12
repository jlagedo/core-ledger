using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;

/// <summary>
///     Query to export historical data for an indexador as CSV with optional date range filtering.
/// </summary>
public record ExportHistoricoIndexadorQuery(
    int IndexadorId,
    DateOnly? DataInicio = null,
    DateOnly? DataFim = null
) : IRequest<ExportHistoricoIndexadorResult>;

/// <summary>
///     Result containing the CSV content and metadata.
/// </summary>
public record ExportHistoricoIndexadorResult(
    string IndexadorCodigo,
    byte[] CsvContent,
    string FileName
);
