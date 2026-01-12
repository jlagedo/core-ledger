using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Transaction-related entities.
/// </summary>
public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        CreateMap<TransactionStatus, TransactionStatusDto>()
            .ConstructUsing(src => new TransactionStatusDto(
                src.Id,
                src.ShortDescription,
                src.LongDescription,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<TransactionType, TransactionTypeDto>()
            .ConstructUsing(src => new TransactionTypeDto(
                src.Id,
                src.ShortDescription,
                src.LongDescription,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<TransactionSubType, TransactionSubTypeDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("TypeId", opt => opt.MapFrom(src => src.TypeId))
            .ForCtorParam("TypeDescription",
                opt => opt.MapFrom(src => src.Type != null ? src.Type.ShortDescription : string.Empty))
            .ForCtorParam("ShortDescription", opt => opt.MapFrom(src => src.ShortDescription))
            .ForCtorParam("LongDescription", opt => opt.MapFrom(src => src.LongDescription))
            .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt))
            .ForCtorParam("UpdatedAt", opt => opt.MapFrom(src => src.UpdatedAt));

        CreateMap<Transaction, TransactionDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("FundId", opt => opt.MapFrom(src => src.FundId))
            .ForCtorParam("FundCode", opt => opt.MapFrom(src => src.Fund != null ? src.Fund.Code : string.Empty))
            .ForCtorParam("FundName", opt => opt.MapFrom(src => src.Fund != null ? src.Fund.Name : string.Empty))
            .ForCtorParam("SecurityId", opt => opt.MapFrom(src => src.SecurityId))
            .ForCtorParam("SecurityTicker",
                opt => opt.MapFrom(src => src.Security != null ? src.Security.Ticker : null))
            .ForCtorParam("SecurityName", opt => opt.MapFrom(src => src.Security != null ? src.Security.Name : null))
            .ForCtorParam("TransactionSubTypeId", opt => opt.MapFrom(src => src.TransactionSubTypeId))
            .ForCtorParam("TransactionSubTypeDescription",
                opt => opt.MapFrom(src =>
                    src.TransactionSubType != null ? src.TransactionSubType.ShortDescription : string.Empty))
            .ForCtorParam("TransactionTypeId",
                opt => opt.MapFrom(src => src.TransactionSubType != null ? src.TransactionSubType.TypeId : 0))
            .ForCtorParam("TransactionTypeDescription",
                opt => opt.MapFrom(src =>
                    src.TransactionSubType != null && src.TransactionSubType.Type != null
                        ? src.TransactionSubType.Type.ShortDescription
                        : string.Empty))
            .ForCtorParam("TradeDate", opt => opt.MapFrom(src => src.TradeDate))
            .ForCtorParam("SettleDate", opt => opt.MapFrom(src => src.SettleDate))
            .ForCtorParam("Quantity", opt => opt.MapFrom(src => src.Quantity))
            .ForCtorParam("Price", opt => opt.MapFrom(src => src.Price))
            .ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount))
            .ForCtorParam("Currency", opt => opt.MapFrom(src => src.Currency))
            .ForCtorParam("StatusId", opt => opt.MapFrom(src => src.StatusId))
            .ForCtorParam("StatusDescription",
                opt => opt.MapFrom(src => src.Status != null ? src.Status.ShortDescription : string.Empty))
            .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt))
            .ForCtorParam("UpdatedAt", opt => opt.MapFrom(src => src.UpdatedAt));
    }
}