using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for FundoParametrosFIDC entity mappings.
/// </summary>
public class FundoParametrosFIDCMappingProfile : Profile
{
    public FundoParametrosFIDCMappingProfile()
    {
        CreateMap<FundoParametrosFIDC, FundoParametrosFIDCDto>()
            .ConstructUsing(src => new FundoParametrosFIDCDto(
                src.Id,
                src.FundoId,
                src.TipoFidc,
                src.TipoFidc.ToString(),
                src.TiposRecebiveis,
                src.TiposRecebiveis.Select(t => t.ToString()).ToList(),
                src.PrazoMedioCarteira,
                src.IndiceSubordinacaoAlvo,
                src.IndiceSubordinacaoMinimo,
                src.ProvisaoDevedoresDuvidosos,
                src.LimiteConcentracaoCedente,
                src.LimiteConcentracaoSacado,
                src.PossuiCoobrigacao,
                src.PercentualCoobrigacao,
                src.RegistradoraRecebiveis,
                src.RegistradoraRecebiveis != null ? src.RegistradoraRecebiveis.ToString() : null,
                src.IntegracaoRegistradora,
                src.CodigoRegistradora,
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}
