using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for CoreJob entity mappings.
/// </summary>
public class CoreJobMappingProfile : Profile
{
    public CoreJobMappingProfile()
    {
        CreateMap<CoreJob, CoreJobDto>()
            .ConstructUsing(src => new CoreJobDto(
                src.Id,
                src.ReferenceId,
                src.Status,
                src.Status.ToString(),
                src.JobDescription,
                src.CreationDate,
                src.RunningDate,
                src.FinishedDate,
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}