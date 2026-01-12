using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Representa uma mensagem de caixa de saída para eventos de criação de transações.
///     Implementa o padrão Transactional Outbox para garantir publicação confiável de mensagens.
/// </summary>
public class TransactionCreatedOutboxMessage
{
    /// <summary>
    ///     Construtor privado para EF Core.
    /// </summary>
    private TransactionCreatedOutboxMessage()
    {
        Type = string.Empty;
        Payload = [];
    }

    /// <summary>
    ///     Identificador único para o registro de mensagem de caixa de saída.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Data e hora em que o evento ocorreu (UTC).
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    ///     Tipo do evento (nome de classe totalmente qualificado).
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    ///     Carga de mensagem serializada (formato binário Protobuf).
    /// </summary>
    public byte[] Payload { get; private set; }

    /// <summary>
    ///     Status de processamento atual da mensagem de caixa de saída.
    /// </summary>
    public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;

    /// <summary>
    ///     Número de vezes que a publicação foi tentada.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    ///     Mensagem de erro da última tentativa de publicação que falhou.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    ///     Data e hora em que a mensagem foi publicada com sucesso (UTC).
    /// </summary>
    public DateTime? PublishedOn { get; private set; }

    /// <summary>
    ///     Método factory para criar uma nova mensagem de caixa de saída de transação criada.
    /// </summary>
    /// <param name="type">Nome de tipo totalmente qualificado do evento.</param>
    /// <param name="payload">Carga de evento serializada em Protobuf.</param>
    /// <param name="occurredOn">Data e hora opcional de quando o evento ocorreu (padrão: agora em UTC).</param>
    /// <returns>Uma nova instância de TransactionCreatedOutboxMessage.</returns>
    /// <exception cref="ArgumentException">Lançada quando tipo ou carga é inválida.</exception>
    public static TransactionCreatedOutboxMessage Create(string type, byte[] payload, DateTime? occurredOn = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Tipo de evento não pode estar vazio.", nameof(type));

        if (payload == null || payload.Length == 0)
            throw new ArgumentException("Carga não pode ser nula ou vazia.", nameof(payload));

        return new TransactionCreatedOutboxMessage
        {
            Type = type.Trim(),
            Payload = payload,
            OccurredOn = occurredOn ?? DateTime.UtcNow,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0
        };
    }

    /// <summary>
    ///     Marca a mensagem como publicada com sucesso.
    /// </summary>
    /// <exception cref="DomainValidationException">Lançada quando a mensagem já foi publicada.</exception>
    public void MarkAsPublished()
    {
        if (Status == OutboxMessageStatus.Published)
            throw new DomainValidationException("Mensagem já foi publicada.");

        Status = OutboxMessageStatus.Published;
        PublishedOn = DateTime.UtcNow;
    }

    /// <summary>
    ///     Registra uma tentativa de publicação que falhou com detalhes do erro.
    /// </summary>
    /// <param name="errorMessage">Descrição do erro que ocorreu.</param>
    /// <exception cref="ArgumentException">Lançada quando a mensagem de erro está vazia.</exception>
    public void RecordFailure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Mensagem de erro não pode estar vazia.", nameof(errorMessage));

        RetryCount++;
        LastError = errorMessage.Trim();
        Status = OutboxMessageStatus.Failed;
    }

    /// <summary>
    ///     Reseta a mensagem para retry após uma tentativa de publicação que falhou.
    /// </summary>
    /// <exception cref="DomainValidationException">Lançada quando a mensagem já foi publicada.</exception>
    public void ResetForRetry()
    {
        if (Status == OutboxMessageStatus.Published)
            throw new DomainValidationException("Não é possível tentar novamente uma mensagem publicada.");

        Status = OutboxMessageStatus.Pending;
        LastError = null;
    }
}
