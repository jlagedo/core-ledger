using AutoMapper;
using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Domain.Cadastros.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for FundoTaxa entity mappings.
/// </summary>
public class FundoTaxaMappingProfile : Profile
{
    public FundoTaxaMappingProfile()
    {
        CreateMap<FundoTaxa, FundoTaxaResponseDto>()
            .ConstructUsing(src => new FundoTaxaResponseDto(
                src.Id,
                src.FundoId,
                src.ClasseId,
                src.TipoTaxa,
                src.TipoTaxa.ToString(),
                src.Percentual,
                src.BaseCalculo,
                src.BaseCalculo.ToString(),
                src.PeriodicidadeProvisao,
                src.PeriodicidadeProvisao.ToString(),
                src.PeriodicidadePagamento,
                src.PeriodicidadePagamento.ToString(),
                src.DiaPagamento,
                src.ValorMinimo,
                src.ValorMaximo,
                src.DataInicioVigencia,
                src.DataFimVigencia,
                src.Ativa,
                src.ParametrosPerformance != null
                    ? new FundoTaxaPerformanceResponseDto(
                        src.ParametrosPerformance.Id,
                        src.ParametrosPerformance.IndexadorId,
                        src.ParametrosPerformance.Indexador != null ? src.ParametrosPerformance.Indexador.Nome : null,
                        src.ParametrosPerformance.PercentualBenchmark,
                        src.ParametrosPerformance.MetodoCalculo,
                        src.ParametrosPerformance.MetodoCalculo.ToString(),
                        src.ParametrosPerformance.LinhaDagua,
                        src.ParametrosPerformance.PeriodicidadeCristalizacao,
                        src.ParametrosPerformance.PeriodicidadeCristalizacao.ToString(),
                        src.ParametrosPerformance.MesCristalizacao
                    )
                    : null,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<FundoTaxa, FundoTaxaListDto>()
            .ConstructUsing(src => new FundoTaxaListDto(
                src.Id,
                src.FundoId,
                src.ClasseId,
                src.TipoTaxa,
                src.TipoTaxa.ToString(),
                src.Percentual,
                src.DataInicioVigencia,
                src.Ativa
            ));
    }
}
