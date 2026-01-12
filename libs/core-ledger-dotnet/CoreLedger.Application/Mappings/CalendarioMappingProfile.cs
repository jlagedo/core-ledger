using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Calendario entity mappings.
/// </summary>
public class CalendarioMappingProfile : Profile
{
    public CalendarioMappingProfile()
    {
        CreateMap<Calendario, CalendarioDto>()
            .ConstructUsing(src => new CalendarioDto(
                src.Id,
                src.Data,
                src.DiaUtil,
                src.TipoDia,
                src.TipoDia.ToString(), // TipoDiaDescricao
                src.Praca,
                src.Praca.ToString(),   // PracaDescricao
                src.Descricao,
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}
