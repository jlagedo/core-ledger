using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Instituicao queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface IInstituicaoQueryService
{
    /// <summary>
    ///     Gets instituicoes with RFC-8040 compliant filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="search">Optional search term for CNPJ, razão social, or nome fantasia.</param>
    /// <param name="ativo">Optional filter by active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of instituicoes list and total count for pagination.</returns>
    Task<(IReadOnlyList<Instituicao> Instituicoes, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        string? search = null,
        bool? ativo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets an instituicao by CNPJ.
    /// </summary>
    /// <param name="cnpj">The CNPJ to search for (14 digits, no formatting).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The instituicao if found, null otherwise.</returns>
    Task<Instituicao?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken = default);
}
