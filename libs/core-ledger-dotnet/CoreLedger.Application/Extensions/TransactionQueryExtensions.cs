using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Métodos de extensão para consultas de Transação para encapsular padrões comuns de carregamento de propriedades de navegação.
/// </summary>
public static class TransactionQueryExtensions
{
    /// <summary>
    ///     Inclui todas as propriedades de navegação para a entidade Transação:
    ///     Fund, Security, TransactionSubType (com Type), e Status.
    /// </summary>
    /// <param name="query">A coleção Transação consultável.</param>
    /// <returns>A consultável com todas as propriedades de navegação incluídas.</returns>
    public static IQueryable<Transaction> WithNavigationProperties(this IQueryable<Transaction> query) =>
        query
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType!)
                .ThenInclude(st => st.Type)
            .Include(t => t.Status);
}
