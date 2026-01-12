using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Handler for updating an existing Indexador.
/// </summary>
public class UpdateIndexadorCommandHandler : IRequestHandler<UpdateIndexadorCommand, IndexadorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateIndexadorCommandHandler> _logger;

    public UpdateIndexadorCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateIndexadorCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IndexadorDto> Handle(
        UpdateIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando indexador {Id}", request.Id);

        var indexador = await _context.Indexadores
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (indexador == null)
        {
            _logger.LogWarning("Falha na atualização do indexador: Indexador {Id} não encontrado", request.Id);
            throw new EntityNotFoundException(nameof(Indexador), request.Id);
        }

        indexador.Update(
            request.Nome,
            request.Fonte,
            request.FatorAcumulado,
            request.DataBase,
            request.UrlFonte,
            request.ImportacaoAutomatica,
            request.Ativo);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Indexador {Id} atualizado", indexador.Id);

        // Get the latest historico entry and count
        var historicoStats = await _context.HistoricosIndexadores
            .Where(h => h.IndexadorId == request.Id)
            .GroupBy(h => h.IndexadorId)
            .Select(g => new
            {
                UltimoValor = g.OrderByDescending(h => h.DataReferencia).Select(h => (decimal?)h.Valor).FirstOrDefault(),
                UltimaData = g.OrderByDescending(h => h.DataReferencia).Select(h => (DateTime?)h.DataReferencia).FirstOrDefault(),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new IndexadorDto(
            indexador.Id,
            indexador.Codigo,
            indexador.Nome,
            indexador.Tipo,
            indexador.Tipo.ToString(),
            indexador.Fonte,
            indexador.Periodicidade,
            indexador.Periodicidade.ToString(),
            indexador.FatorAcumulado,
            indexador.DataBase,
            indexador.UrlFonte,
            indexador.ImportacaoAutomatica,
            indexador.Ativo,
            indexador.CreatedAt,
            indexador.UpdatedAt,
            historicoStats?.UltimoValor,
            historicoStats?.UltimaData,
            historicoStats?.Count ?? 0
        );
    }
}
