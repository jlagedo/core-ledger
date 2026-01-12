using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Indexador entity mappings.
/// </summary>
public class IndexadorMappingProfile : Profile
{
    public IndexadorMappingProfile()
    {
        // Note: This mapping provides default null/0 values for historico stats.
        // For accurate stats, use manual DTO construction with query service.
        CreateMap<Indexador, IndexadorDto>()
            .ConstructUsing(src => new IndexadorDto(
                src.Id,
                src.Codigo,
                src.Nome,
                src.Tipo,
                src.Tipo.ToString(),
                src.Fonte,
                src.Periodicidade,
                src.Periodicidade.ToString(),
                src.FatorAcumulado,
                src.DataBase,
                src.UrlFonte,
                src.ImportacaoAutomatica,
                src.Ativo,
                src.CreatedAt,
                src.UpdatedAt,
                null,  // UltimoValor - requires separate query
                null,  // UltimaData - requires separate query
                0      // HistoricoCount - requires separate query
            ));
    }
}
