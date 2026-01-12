using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for FundoVinculo entity mappings.
/// </summary>
public class FundoVinculoMappingProfile : Profile
{
    public FundoVinculoMappingProfile()
    {
        CreateMap<FundoVinculo, FundoVinculoDto>()
            .ConstructUsing(src => new FundoVinculoDto(
                src.Id,
                src.FundoId,
                src.InstituicaoId,
                src.Instituicao != null ? src.Instituicao.RazaoSocial : string.Empty,
                src.Instituicao != null ? src.Instituicao.Cnpj.Valor : string.Empty,
                src.TipoVinculo,
                src.TipoVinculo.ToString(),
                src.DataInicio,
                src.DataFim,
                src.ContratoNumero,
                src.Observacao,
                src.Principal,
                src.EstaVigente(),
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}
