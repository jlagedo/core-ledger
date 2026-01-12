using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Account entity mappings.
/// </summary>
public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        CreateMap<Account, AccountDto>()
            .ConstructUsing(src => new AccountDto(
                src.Id,
                src.Code,
                src.Name,
                src.TypeId,
                src.Type != null ? src.Type.Description : string.Empty,
                src.Status,
                src.Status.ToString(),
                src.NormalBalance,
                src.NormalBalance.ToString(),
                src.CreatedAt,
                src.UpdatedAt,
                src.DeactivatedAt
            ));
    }
}