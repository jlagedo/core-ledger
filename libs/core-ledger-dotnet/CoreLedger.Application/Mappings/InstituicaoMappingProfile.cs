using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Instituicao entity mappings.
/// </summary>
public class InstituicaoMappingProfile : Profile
{
    public InstituicaoMappingProfile()
    {
        CreateMap<Instituicao, InstituicaoDto>()
            .ConstructUsing(src => new InstituicaoDto(
                src.Id,
                src.Cnpj.Valor,
                src.RazaoSocial,
                src.NomeFantasia,
                src.Ativo,
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}
