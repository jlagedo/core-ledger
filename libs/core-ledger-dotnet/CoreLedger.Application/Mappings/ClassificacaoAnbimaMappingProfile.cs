using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for ClassificacaoAnbima entity mappings.
/// </summary>
public class ClassificacaoAnbimaMappingProfile : Profile
{
    public ClassificacaoAnbimaMappingProfile()
    {
        CreateMap<ClassificacaoAnbima, ClassificacaoAnbimaDto>()
            .ConstructUsing(src => new ClassificacaoAnbimaDto(
                src.Id,
                src.Codigo,
                src.Nome,
                src.Nivel1,
                src.Nivel2,
                src.Nivel3,
                src.ClassificacaoCvm,
                src.Descricao,
                // Compute NomeCompleto: "Nivel1 > Nivel2 > Nivel3" or "Nivel1 > Nivel2"
                src.Nivel3 != null
                    ? $"{src.Nivel1} > {src.Nivel2} > {src.Nivel3}"
                    : $"{src.Nivel1} > {src.Nivel2}"
            ));
    }
}
