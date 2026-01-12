namespace CoreLedger.Domain.Enums;

/// <summary>
///     Representa o status de processamento de uma mensagem de caixa de saída.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>
    ///     Mensagem aguardando publicação na fila de mensagens.
    /// </summary>
    Pending = 0,

    /// <summary>
    ///     Mensagem foi publicada com sucesso na fila de mensagens.
    /// </summary>
    Published = 1,

    /// <summary>
    ///     A publicação da mensagem falhou após tentativas de repetição.
    /// </summary>
    Failed = 2
}
