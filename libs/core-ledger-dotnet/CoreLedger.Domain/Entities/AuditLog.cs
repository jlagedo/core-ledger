using System.Text.Json;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Representa um registro de auditoria para rastreamento de alterações de entidades.
/// </summary>
public class AuditLog
{
    /// <summary>
    ///     Construtor privado para EF Core.
    /// </summary>
    private AuditLog()
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
        EventType = string.Empty;
    }

    /// <summary>
    ///     Identificador único para o registro de auditoria.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Nome da entidade sendo auditada (ex: 'Transação', 'Conta').
    /// </summary>
    public string EntityName { get; private set; }

    /// <summary>
    ///     Identificador da entidade sendo auditada (suporta chaves UUID, int, string).
    /// </summary>
    public string EntityId { get; private set; }

    /// <summary>
    ///     Tipo de evento que ocorreu (ex: 'Criado', 'Atualizado', 'Deletado').
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    ///     Identificador do usuário que disparou o evento.
    /// </summary>
    public string? PerformedByUserId { get; private set; }

    /// <summary>
    ///     Data e hora em que o evento ocorreu.
    /// </summary>
    public DateTime PerformedAt { get; private set; }

    /// <summary>
    ///     Captura JSON do estado da entidade antes da alteração.
    /// </summary>
    public JsonDocument? DataBefore { get; private set; }

    /// <summary>
    ///     Captura JSON do estado da entidade após a alteração.
    /// </summary>
    public JsonDocument? DataAfter { get; private set; }

    /// <summary>
    ///     ID de correlação para rastreamento distribuído.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    ///     ID de requisição ASP.NET Core.
    /// </summary>
    public string? RequestId { get; private set; }

    /// <summary>
    ///     Origem do evento (ex: 'API', 'Job', 'Sistema').
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    ///     Método factory para criar um novo registro de auditoria.
    /// </summary>
    /// <param name="entityName">Nome da entidade sendo auditada.</param>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <param name="eventType">Tipo de evento (Criado, Atualizado, Deletado).</param>
    /// <param name="performedByUserId">Usuário que disparou o evento.</param>
    /// <param name="dataBefore">Captura JSON antes da alteração.</param>
    /// <param name="dataAfter">Captura JSON após a alteração.</param>
    /// <param name="correlationId">ID de correlação para rastreamento.</param>
    /// <param name="requestId">ID de requisição ASP.NET Core.</param>
    /// <param name="source">Origem do evento.</param>
    /// <returns>Uma nova instância de AuditLog.</returns>
    public static AuditLog Create(
        string entityName,
        string entityId,
        string eventType,
        string? performedByUserId = null,
        JsonDocument? dataBefore = null,
        JsonDocument? dataAfter = null,
        string? correlationId = null,
        string? requestId = null,
        string? source = null)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Nome da entidade é obrigatório.", nameof(entityName));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("ID da entidade é obrigatório.", nameof(entityId));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Tipo de evento é obrigatório.", nameof(eventType));

        return new AuditLog
        {
            EntityName = entityName.Trim(),
            EntityId = entityId.Trim(),
            EventType = eventType.Trim(),
            PerformedByUserId = performedByUserId?.Trim(),
            PerformedAt = DateTime.UtcNow,
            DataBefore = dataBefore,
            DataAfter = dataAfter,
            CorrelationId = correlationId?.Trim(),
            RequestId = requestId?.Trim(),
            Source = source?.Trim()
        };
    }
}
