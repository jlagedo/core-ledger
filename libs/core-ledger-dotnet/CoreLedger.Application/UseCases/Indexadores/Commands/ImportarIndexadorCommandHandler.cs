using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Handler for triggering automatic indexador data import.
/// </summary>
public class ImportarIndexadorCommandHandler : IRequestHandler<ImportarIndexadorCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ImportarIndexadorCommandHandler> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public ImportarIndexadorCommandHandler(
        IApplicationDbContext context,
        IMessagePublisher messagePublisher,
        ILogger<ImportarIndexadorCommandHandler> logger)
    {
        _context = context;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ImportarIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disparando importação para indexador {Id}", request.Id);

        var indexador = await _context.Indexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (indexador == null)
        {
            _logger.LogWarning("Falha na importação: Indexador {Id} não encontrado", request.Id);
            throw new EntityNotFoundException(nameof(Indexador), request.Id);
        }

        // Validate importacao automatica is enabled
        if (!indexador.ImportacaoAutomatica)
        {
            _logger.LogWarning("Falha na importação: Indexador {Id} não tem importação automática habilitada", request.Id);
            throw new DomainValidationException(
                $"Indexador '{indexador.Codigo}' não tem importação automática habilitada");
        }

        // Validate URL fonte exists
        if (string.IsNullOrWhiteSpace(indexador.UrlFonte))
        {
            _logger.LogWarning("Falha na importação: Indexador {Id} não tem URL fonte configurada", request.Id);
            throw new DomainValidationException(
                $"Indexador '{indexador.Codigo}' não tem URL fonte configurada");
        }

        // Publish message to RabbitMQ for Worker processing
        var message = new IndexadorImportMessage(
            indexador.Id,
            indexador.UrlFonte,
            request.CorrelationId);

        await _messagePublisher.PublishAsync(
            "indexador.import",
            message,
            request.CorrelationId,
            cancellationToken);

        _logger.LogInformation(
            "Mensagem de importação publicada para indexador {Id} na fila 'indexador.import' com ID de correlação {CorrelationId}",
            indexador.Id, request.CorrelationId);

        return Unit.Value;
    }
}
