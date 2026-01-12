using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Query to get hierarchical levels (Nivel1, Nivel2 grouped by Nivel1) for ANBIMA classifications.
/// </summary>
public record ListarNiveisAnbimaQuery(
    string? ClassificacaoCvm = null
) : IRequest<NiveisClassificacaoAnbimaResponse>;
