using AutoMapper;
using CoreLedger.Application.DTOs.FundoPrazo;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for FundoPrazo entity mappings.
/// </summary>
public class FundoPrazoMappingProfile : Profile
{
    public FundoPrazoMappingProfile()
    {
        CreateMap<FundoPrazo, FundoPrazoResponseDto>()
            .ConstructUsing(src => new FundoPrazoResponseDto(
                src.Id,
                src.FundoId,
                src.ClasseId,
                src.TipoPrazo,
                src.TipoPrazo.ToString(),
                src.DiasCotizacao,
                src.DiasLiquidacao,
                src.DiasCarencia,
                src.HorarioLimite,
                src.DiasUteis,
                src.CalendarioId,
                src.PermiteParcial,
                src.PercentualMinimo,
                src.ValorMinimo,
                src.Ativo,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<FundoPrazo, FundoPrazoListDto>()
            .ConstructUsing(src => new FundoPrazoListDto(
                src.Id,
                src.FundoId,
                src.ClasseId,
                src.TipoPrazo,
                src.TipoPrazo.ToString(),
                src.DiasCotizacao,
                src.DiasLiquidacao,
                src.HorarioLimite,
                src.Ativo
            ));
    }
}
