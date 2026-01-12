using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Fundo (cadastros) queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public interface IFundoQueryService
{
    /// <summary>
    ///     Gets fundos with RFC-8040 compliant filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters including filter, sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of fundos list and total count for pagination.</returns>
    Task<(IReadOnlyList<Fundo> Fundos, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches fundos for text search using PostgreSQL ILIKE.
    /// </summary>
    /// <param name="searchTerm">The search term to match against razao_social, nome_fantasia, or nome_curto.</param>
    /// <param name="limit">Maximum number of results (default: 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching fundos.</returns>
    Task<IReadOnlyList<Fundo>> SearchAsync(
        string searchTerm,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a fundo by CNPJ.
    /// </summary>
    /// <param name="cnpj">The CNPJ to search for (14 digits, no formatting).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fundo if found, null otherwise.</returns>
    Task<Fundo?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken = default);
}
