using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;

/// <summary>
///     Query to list ANBIMA classifications with optional filters.
/// </summary>
public record ListarClassificacoesAnbimaQuery(
    string? ClassificacaoCvm = null,
    string? Nivel1 = null,
    bool Ativo = true
) : IRequest<ListarClassificacoesAnbimaResponse>;
