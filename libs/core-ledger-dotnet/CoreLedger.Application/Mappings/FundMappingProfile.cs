using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy support

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Fund entity mappings.
/// </summary>
public class FundMappingProfile : Profile
{
    public FundMappingProfile()
    {
        CreateMap<Fund, FundDto>()
            .ConstructUsing(src => new FundDto(
                src.Id,
                src.Code,
                src.Name,
                src.BaseCurrency,
                src.InceptionDate,
                src.ValuationFrequency,
                src.ValuationFrequency.ToString(),
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}