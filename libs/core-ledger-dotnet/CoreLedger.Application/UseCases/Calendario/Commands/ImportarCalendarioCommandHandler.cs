using CoreLedger.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Handler for ImportarCalendarioCommand.
///     STUB: Returns empty result for now.
/// </summary>
public class ImportarCalendarioCommandHandler : IRequestHandler<ImportarCalendarioCommand, ImportarCalendarioResultDto>
{
    private readonly ILogger<ImportarCalendarioCommandHandler> _logger;

    public ImportarCalendarioCommandHandler(ILogger<ImportarCalendarioCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<ImportarCalendarioResultDto> Handle(
        ImportarCalendarioCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "ImportarCalendarioCommand ainda não está implementado - solicitado para ano {Ano}, praça {Praca}",
            request.Ano,
            request.Praca);

        // STUB: Return empty result
        var result = new ImportarCalendarioResultDto(
            Ano: request.Ano,
            DiasImportados: 0,
            DiasAtualizados: 0);

        return Task.FromResult(result);
    }
}
