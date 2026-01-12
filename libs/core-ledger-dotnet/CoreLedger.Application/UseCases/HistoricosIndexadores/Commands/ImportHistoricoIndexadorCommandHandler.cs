using System.Globalization;
using System.Text;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;

/// <summary>
///     Handler for ImportHistoricoIndexadorCommand.
///     Parses CSV file and imports historical data.
/// </summary>
public class ImportHistoricoIndexadorCommandHandler : IRequestHandler<ImportHistoricoIndexadorCommand, ImportHistoricoIndexadorResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ImportHistoricoIndexadorCommandHandler> _logger;

    public ImportHistoricoIndexadorCommandHandler(
        IApplicationDbContext context,
        ILogger<ImportHistoricoIndexadorCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportHistoricoIndexadorResult> Handle(
        ImportHistoricoIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        // Verify indexador exists
        var indexador = await _context.Indexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.IndexadorId, cancellationToken);

        if (indexador == null)
        {
            throw new EntityNotFoundException(nameof(Indexador), request.IndexadorId);
        }

        _logger.LogInformation(
            "Starting CSV import for indexador {IndexadorId} ({Codigo}), Sobrescrever: {Sobrescrever}",
            request.IndexadorId, indexador.Codigo, request.Sobrescrever);

        // Parse CSV content
        var csvText = Encoding.UTF8.GetString(request.CsvContent);
        var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            throw new DomainValidationException("CSV deve conter pelo menos o cabeçalho e uma linha de dados");
        }

        // Validate header
        var header = lines[0].ToLowerInvariant();
        if (!header.Contains("data_referencia") || !header.Contains("valor"))
        {
            throw new DomainValidationException(
                "Cabeçalho CSV inválido. Formato esperado: data_referencia;valor;fator_diario;variacao_percentual;fonte");
        }

        // Get existing dates for this indexador
        var existingDates = await _context.HistoricosIndexadores
            .Where(h => h.IndexadorId == request.IndexadorId)
            .Select(h => h.DataReferencia.Date)
            .ToHashSetAsync(cancellationToken);

        var errors = new List<string>();
        var toAdd = new List<HistoricoIndexador>();
        var toDelete = new List<HistoricoIndexador>();
        var skippedRows = 0;
        var overwrittenRows = 0;
        var totalDataRows = lines.Length - 1;

        // Parse data rows
        for (var i = 1; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var row = ParseCsvRow(line, lineNumber);

                if (existingDates.Contains(row.DataReferencia.Date))
                {
                    if (request.Sobrescrever)
                    {
                        // HistoricoIndexador is immutable, so delete and recreate
                        var existing = await _context.HistoricosIndexadores
                            .FirstOrDefaultAsync(h =>
                                h.IndexadorId == request.IndexadorId &&
                                h.DataReferencia.Date == row.DataReferencia.Date,
                                cancellationToken);

                        if (existing != null)
                        {
                            toDelete.Add(existing);
                            overwrittenRows++;
                        }

                        // Create new record with same date
                        var historico = HistoricoIndexador.Create(
                            request.IndexadorId,
                            row.DataReferencia,
                            row.Valor,
                            row.FatorDiario,
                            row.VariacaoPercentual,
                            row.Fonte ?? "IMPORTACAO",
                            null);

                        toAdd.Add(historico);
                    }
                    else
                    {
                        skippedRows++;
                        _logger.LogDebug(
                            "Skipping duplicate date {Date} at line {Line} (sobrescrever=false)",
                            row.DataReferencia.Date, lineNumber);
                    }
                }
                else
                {
                    var historico = HistoricoIndexador.Create(
                        request.IndexadorId,
                        row.DataReferencia,
                        row.Valor,
                        row.FatorDiario,
                        row.VariacaoPercentual,
                        row.Fonte ?? "IMPORTACAO",
                        null);

                    toAdd.Add(historico);
                    existingDates.Add(row.DataReferencia.Date);
                }
            }
            catch (Exception ex) when (ex is not EntityNotFoundException)
            {
                errors.Add($"Linha {lineNumber}: {ex.Message}");
                _logger.LogWarning(ex, "Error parsing CSV line {Line}: {Content}", lineNumber, line);
            }
        }

        // Delete records that will be overwritten
        if (toDelete.Count > 0)
        {
            _context.HistoricosIndexadores.RemoveRange(toDelete);
        }

        // Add new records
        if (toAdd.Count > 0)
        {
            _context.HistoricosIndexadores.AddRange(toAdd);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var importedRows = toAdd.Count - overwrittenRows; // New records only

        _logger.LogInformation(
            "CSV import completed for indexador {IndexadorId}: Total={Total}, Imported={Imported}, Updated={Updated}, Skipped={Skipped}, Errors={Errors}",
            request.IndexadorId, totalDataRows, importedRows, overwrittenRows, skippedRows, errors.Count);

        return new ImportHistoricoIndexadorResult(
            totalDataRows,
            importedRows,
            skippedRows,
            overwrittenRows,
            errors);
    }

    private static CsvRow ParseCsvRow(string line, int lineNumber)
    {
        // Support both semicolon and comma as delimiters
        var delimiter = line.Contains(';') ? ';' : ',';
        var parts = line.Split(delimiter);

        if (parts.Length < 2)
        {
            throw new DomainValidationException(
                $"Formato inválido. Esperados pelo menos 2 campos (data_referencia, valor)");
        }

        // Parse data_referencia (required)
        if (!DateTime.TryParse(parts[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataReferencia))
        {
            throw new DomainValidationException($"Data inválida: '{parts[0]}'");
        }

        // Parse valor (required)
        if (!decimal.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
        {
            throw new DomainValidationException($"Valor inválido: '{parts[1]}'");
        }

        // Parse fator_diario (optional)
        decimal? fatorDiario = null;
        if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
        {
            if (decimal.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var fd))
            {
                fatorDiario = fd;
            }
        }

        // Parse variacao_percentual (optional)
        decimal? variacaoPercentual = null;
        if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
        {
            if (decimal.TryParse(parts[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vp))
            {
                variacaoPercentual = vp;
            }
        }

        // Parse fonte (optional)
        string? fonte = null;
        if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
        {
            fonte = parts[4].Trim();
        }

        return new CsvRow(dataReferencia, valor, fatorDiario, variacaoPercentual, fonte);
    }

    private record CsvRow(
        DateTime DataReferencia,
        decimal Valor,
        decimal? FatorDiario,
        decimal? VariacaoPercentual,
        string? Fonte);
}
