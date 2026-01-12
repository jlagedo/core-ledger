namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade base para todas as entidades de domínio com propriedades comuns.
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Identificador único da entidade.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    ///     Data e hora de criação da entidade.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    ///     Data e hora da última atualização da entidade.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    ///     Define a data de atualização como a data/hora atual.
    /// </summary>
    public void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}