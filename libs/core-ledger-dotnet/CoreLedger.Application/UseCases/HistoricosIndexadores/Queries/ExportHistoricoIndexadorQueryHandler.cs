using System.Globalization;
using System.Text;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;

/// <summary>
///     Handler for ExportHistoricoIndexadorQuery.
///     Generates CSV file with all historical data for an indexador.
/// </summary>
public class ExportHistoricoIndexadorQueryHandler : IRequestHandler<ExportHistoricoIndexadorQuery, ExportHistoricoIndexadorResult>
{
    private readonly IApplicationDbContext _context;

    public ExportHistoricoIndexadorQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExportHistoricoIndexadorResult> Handle(
        ExportHistoricoIndexadorQuery request,
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

        // Build query for historical data
        var query = _context.HistoricosIndexadores
            .AsNoTracking()
            .Where(h => h.IndexadorId == request.IndexadorId);

        // Apply date range filters
        if (request.DataInicio.HasValue)
        {
            var dataInicio = request.DataInicio.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(h => h.DataReferencia >= dataInicio);
        }

        if (request.DataFim.HasValue)
        {
            var dataFim = request.DataFim.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(h => h.DataReferencia <= dataFim);
        }

        // Order by date ascending for export
        var historicos = await query
            .OrderBy(h => h.DataReferencia)
            .ToListAsync(cancellationToken);

        // Generate CSV content
        var csv = GenerateCsv(historicos);
        var csvBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

        // Generate filename
        var dateRange = "";
        if (request.DataInicio.HasValue || request.DataFim.HasValue)
        {
            var start = request.DataInicio?.ToString("yyyyMMdd") ?? "inicio";
            var end = request.DataFim?.ToString("yyyyMMdd") ?? "fim";
            dateRange = $"_{start}_{end}";
        }
        var fileName = $"{indexador.Codigo.ToLowerInvariant()}_historico{dateRange}.csv";

        return new ExportHistoricoIndexadorResult(indexador.Codigo, csvBytes, fileName);
    }

    private static string GenerateCsv(List<HistoricoIndexador> historicos)
    {
        var sb = new StringBuilder();

        // CSV Header with semicolon delimiter (common in Brazilian systems)
        sb.AppendLine("data_referencia;valor;fator_diario;variacao_percentual;fonte");

        // Data rows
        foreach (var h in historicos)
        {
            var dataReferencia = h.DataReferencia.ToString("yyyy-MM-dd");
            var valor = h.Valor.ToString("F8", CultureInfo.InvariantCulture);
            var fatorDiario = h.FatorDiario?.ToString("F12", CultureInfo.InvariantCulture) ?? "";
            var variacaoPercentual = h.VariacaoPercentual?.ToString("F6", CultureInfo.InvariantCulture) ?? "";
            var fonte = h.Fonte ?? "";

            sb.AppendLine($"{dataReferencia};{valor};{fatorDiario};{variacaoPercentual};{fonte}");
        }

        return sb.ToString();
    }
}
