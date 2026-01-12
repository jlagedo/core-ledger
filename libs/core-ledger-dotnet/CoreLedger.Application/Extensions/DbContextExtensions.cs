using CoreLedger.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Métodos de extensão para DbSet para encapsular padrões comuns de acesso a dados.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    ///     Valida que uma entidade com o ID especificado existe no banco de dados.
    ///     Lança EntityNotFoundException se a entidade não for encontrada.
    /// </summary>
    /// <typeparam name="TEntity">O tipo de entidade a validar.</typeparam>
    /// <param name="dbSet">O DbSet a pesquisar.</param>
    /// <param name="keyValues">Os valores de chave primária a pesquisar.</param>
    /// <param name="entityTypeName">O nome do tipo de entidade para mensagens de erro.</param>
    /// <param name="logger">Registrador para mensagens de diagnóstico.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A entidade encontrada.</returns>
    /// <exception cref="EntityNotFoundException">Lançado quando a entidade não é encontrada.</exception>
    public static async Task<TEntity> ValidateEntityExistsAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        object[] keyValues,
        string entityTypeName,
        ILogger logger,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var entity = await dbSet.FindAsync(keyValues, cancellationToken);

        if (entity == null)
        {
            var keyValue = keyValues.Length == 1 ? keyValues[0] : string.Join(", ", keyValues);
            logger.LogWarning("Validação falhou: {EntityType} {KeyValue} não encontrado",
                entityTypeName, keyValue);
            throw new EntityNotFoundException(entityTypeName, keyValue);
        }

        return entity;
    }
}
