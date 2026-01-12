using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Query to verify compatibility between an ANBIMA classification code and a CVM classification.
/// </summary>
public record VerificarCompatibilidadeAnbimaQuery(
    string CodigoAnbima,
    string ClassificacaoCvm
) : IRequest<VerificarCompatibilidadeResponse>;
