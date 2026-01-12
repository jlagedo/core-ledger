using AutoMapper;
using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for FundoClasse entity mappings.
/// </summary>
public class FundoClasseMappingProfile : Profile
{
    public FundoClasseMappingProfile()
    {
        CreateMap<FundoClasse, FundoClasseResponseDto>()
            .ConstructUsing(src => new FundoClasseResponseDto(
                src.Id,
                src.FundoId,
                src.CodigoClasse,
                src.NomeClasse,
                src.CnpjClasse,
                src.TipoClasseFidc,
                src.TipoClasseFidc != null ? src.TipoClasseFidc.ToString() : null,
                src.OrdemSubordinacao,
                src.RentabilidadeAlvo,
                src.ResponsabilidadeLimitada,
                src.SegregacaoPatrimonial,
                src.ValorMinimoAplicacao,
                src.Ativa,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<FundoClasse, FundoClasseListDto>()
            .ConstructUsing(src => new FundoClasseListDto(
                src.Id,
                src.FundoId,
                src.CodigoClasse,
                src.NomeClasse,
                src.TipoClasseFidc,
                src.TipoClasseFidc != null ? src.TipoClasseFidc.ToString() : null,
                src.Ativa
            ));
    }
}
