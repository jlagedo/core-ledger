using AutoMapper;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Fundo entity mappings.
/// </summary>
public class FundoMappingProfile : Profile
{
    public FundoMappingProfile()
    {
        CreateMap<Fundo, FundoResponseDto>()
            .ConstructUsing(src => new FundoResponseDto(
                src.Id,
                src.Cnpj.Valor,
                src.Cnpj.Formatado,
                src.RazaoSocial,
                src.NomeFantasia,
                src.NomeCurto,
                src.DataConstituicao,
                src.DataInicioAtividade,
                src.TipoFundo,
                src.TipoFundo.ToString(),
                src.ClassificacaoCVM,
                src.ClassificacaoCVM.ToString(),
                src.ClassificacaoAnbima,
                src.CodigoAnbima != null ? src.CodigoAnbima.Valor : null,
                src.Situacao,
                src.Situacao.ToString(),
                src.Prazo,
                src.Prazo.ToString(),
                src.PublicoAlvo,
                src.PublicoAlvo.ToString(),
                src.Tributacao,
                src.Tributacao.ToString(),
                src.Condominio,
                src.Condominio.ToString(),
                src.Exclusivo,
                src.Reservado,
                src.PermiteAlavancagem,
                src.AceitaCripto,
                src.PercentualExterior,
                src.WizardCompleto,
                src.ProgressoCadastro,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<Fundo, FundoListDto>()
            .ConstructUsing(src => new FundoListDto(
                src.Id,
                src.Cnpj.Valor,
                src.Cnpj.Formatado,
                src.RazaoSocial,
                src.NomeCurto,
                src.TipoFundo,
                src.TipoFundo.ToString(),
                src.Situacao,
                src.Situacao.ToString(),
                src.ProgressoCadastro
            ));
    }
}
