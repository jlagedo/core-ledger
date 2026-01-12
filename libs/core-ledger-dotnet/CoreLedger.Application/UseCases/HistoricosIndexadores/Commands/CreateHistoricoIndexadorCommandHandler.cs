using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;

/// <summary>
///     Handler for CreateHistoricoIndexadorCommand.
///     Validates indexador exists and uniqueness of (indexador_id, data_referencia).
/// </summary>
public class CreateHistoricoIndexadorCommandHandler : IRequestHandler<CreateHistoricoIndexadorCommand, HistoricoIndexadorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateHistoricoIndexadorCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<HistoricoIndexadorDto> Handle(CreateHistoricoIndexadorCommand request, CancellationToken cancellationToken)
    {
        // Validate indexador exists
        var indexadorExists = await _context.Indexadores
            .AsNoTracking()
            .AnyAsync(i => i.Id == request.IndexadorId, cancellationToken);

        if (!indexadorExists)
        {
            throw new EntityNotFoundException("Indexador", request.IndexadorId);
        }

        // Check uniqueness of (indexador_id, data_referencia)
        var duplicate = await _context.HistoricosIndexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(
                h => h.IndexadorId == request.IndexadorId && h.DataReferencia.Date == request.DataReferencia.Date,
                cancellationToken);

        if (duplicate != null)
        {
            throw new DomainValidationException(
                $"Registro histórico para indexador {request.IndexadorId} em data {request.DataReferencia:yyyy-MM-dd} já existe");
        }

        // Create entity via factory method
        var historico = HistoricoIndexador.Create(
            request.IndexadorId,
            request.DataReferencia,
            request.Valor,
            request.FatorDiario,
            request.VariacaoPercentual,
            request.Fonte,
            request.ImportacaoId);

        _context.HistoricosIndexadores.Add(historico);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<HistoricoIndexadorDto>(historico);
    }
}
