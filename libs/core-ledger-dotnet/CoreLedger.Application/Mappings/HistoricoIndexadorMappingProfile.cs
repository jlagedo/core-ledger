using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for HistoricoIndexador entity mappings.
/// </summary>
public class HistoricoIndexadorMappingProfile : Profile
{
    public HistoricoIndexadorMappingProfile()
    {
        CreateMap<HistoricoIndexador, HistoricoIndexadorDto>()
            .ConstructUsing(src => new HistoricoIndexadorDto(
                src.Id,
                src.IndexadorId,
                src.Indexador != null ? src.Indexador.Codigo : string.Empty,
                src.DataReferencia,
                src.Valor,
                src.FatorDiario,
                src.VariacaoPercentual,
                src.Fonte,
                src.ImportacaoId,
                src.CreatedAt
            ));
    }
}
