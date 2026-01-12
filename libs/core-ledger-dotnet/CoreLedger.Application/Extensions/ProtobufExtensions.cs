using ProtoBuf;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Métodos de extensão para operações de serialização Protobuf.
/// </summary>
public static class ProtobufExtensions
{
    /// <summary>
    ///     Serializa uma mensagem para formato binário Protobuf.
    /// </summary>
    /// <typeparam name="T">O tipo da mensagem a serializar.</typeparam>
    /// <param name="message">A instância da mensagem a serializar.</param>
    /// <returns>Array de bytes contendo a mensagem serializada em formato Protobuf.</returns>
    public static byte[] SerializeToProtobuf<T>(this T message)
    {
        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, message);
        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Desserializa um payload binário Protobuf para uma mensagem fortemente tipada.
    /// </summary>
    /// <typeparam name="T">O tipo da mensagem a desserializar.</typeparam>
    /// <param name="payload">Array de bytes contendo a mensagem serializada em Protobuf.</param>
    /// <returns>Instância da mensagem desserializada.</returns>
    /// <exception cref="ArgumentException">Lançado quando o payload é nulo ou vazio.</exception>
    /// <exception cref="InvalidOperationException">Lançado quando a desserialização falha.</exception>
    public static T DeserializeFromProtobuf<T>(this byte[] payload)
    {
        if (payload == null || payload.Length == 0)
            throw new ArgumentException("Payload não pode ser nulo ou vazio.", nameof(payload));

        try
        {
            using var memoryStream = new MemoryStream(payload);
            return Serializer.Deserialize<T>(memoryStream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Falha ao desserializar payload Protobuf para o tipo {typeof(T).Name}. " +
                $"Tamanho do payload: {payload.Length} bytes.", ex);
        }
    }
}
