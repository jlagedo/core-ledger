using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Security entity mappings.
/// </summary>
public class SecurityMappingProfile : Profile
{
    public SecurityMappingProfile()
    {
        CreateMap<Security, SecurityDto>()
            .ConstructUsing(src => new SecurityDto(
                src.Id,
                src.Name,
                src.Ticker,
                src.Isin,
                src.Type,
                src.Type.ToString(),
                src.Currency,
                src.Status,
                src.Status.ToString(),
                src.CreatedAt,
                src.UpdatedAt,
                src.DeactivatedAt
            ));
    }
}