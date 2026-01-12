using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Handler for VerificarCnpjDisponivelQuery.
///     Checks if a CNPJ is available for fund registration.
/// </summary>
public class VerificarCnpjDisponivelQueryHandler
    : IRequestHandler<VerificarCnpjDisponivelQuery, CnpjDisponibilidadeResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<VerificarCnpjDisponivelQueryHandler> _logger;

    public VerificarCnpjDisponivelQueryHandler(
        IApplicationDbContext context,
        ILogger<VerificarCnpjDisponivelQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CnpjDisponibilidadeResponseDto> Handle(
        VerificarCnpjDisponivelQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking CNPJ availability: {Cnpj}", request.Cnpj);

        // Validate CNPJ format and checksum
        if (!CNPJ.TentarCriar(request.Cnpj, out var cnpjVO) || cnpjVO is null)
        {
            _logger.LogWarning("Invalid CNPJ format: {Cnpj}", request.Cnpj);

            return new CnpjDisponibilidadeResponseDto(
                Cnpj: request.Cnpj,
                Disponivel: false,
                CnpjValido: false,
                Mensagem: "CNPJ inválido - formato ou dígitos verificadores incorretos");
        }

        // Check if CNPJ already exists (considering soft delete)
        var exists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Cnpj == cnpjVO && f.DeletedAt == null, cancellationToken);

        if (exists)
        {
            _logger.LogInformation("CNPJ {Cnpj} already exists", cnpjVO.Formatado);
        }
        else
        {
            _logger.LogInformation("CNPJ {Cnpj} is available", cnpjVO.Formatado);
        }

        return new CnpjDisponibilidadeResponseDto(
            Cnpj: cnpjVO.Formatado,  // Return formatted CNPJ
            Disponivel: !exists,
            CnpjValido: true,
            Mensagem: exists ? "CNPJ já cadastrado em outro fundo" : null);
    }
}
