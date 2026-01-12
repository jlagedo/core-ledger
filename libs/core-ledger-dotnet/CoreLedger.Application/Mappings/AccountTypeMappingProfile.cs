using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for AccountType entity mappings.
/// </summary>
public class AccountTypeMappingProfile : Profile
{
    public AccountTypeMappingProfile()
    {
        CreateMap<AccountType, AccountTypeDto>();
    }
}