using System.Text.Json;
using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for AuditLog entity mappings.
/// </summary>
public class AuditLogMappingProfile : Profile
{
    public AuditLogMappingProfile()
    {
        CreateMap<AuditLog, AuditLogDto>()
            .ConstructUsing(src => new AuditLogDto(
                src.Id,
                src.EntityName,
                src.EntityId,
                src.EventType,
                src.PerformedByUserId,
                src.PerformedAt,
                src.DataBefore != null ? src.DataBefore.RootElement.Clone() : null,
                src.DataAfter != null ? src.DataAfter.RootElement.Clone() : null,
                src.CorrelationId,
                src.RequestId,
                src.Source
            ))
            .ForAllMembers(opt => opt.Ignore());
    }
}
