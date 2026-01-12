using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Query to get an ANBIMA classification by its codigo.
/// </summary>
public record ObterClassificacaoAnbimaPorCodigoQuery(
    string Codigo
) : IRequest<ClassificacaoAnbimaDto?>;
