namespace CoreLedger.Domain.Exceptions;

/// <summary>
///     Exceção base para todas as exceções relacionadas ao domínio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}

/// <summary>
///     Exceção lançada quando a validação de domínio falha.
/// </summary>
public class DomainValidationException : DomainException
{
    public DomainValidationException(string message)
        : base(message, "ERR-DOMAIN-001")
    {
    }
}

/// <summary>
///     Exceção lançada quando uma entidade não é encontrada.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} com id {id} não encontrado", "ERR-NOTFOUND-001")
    {
    }
}

/// <summary>
///     Exceção lançada quando um serviço externo (como Auth0) falha.
/// </summary>
public class ExternalServiceException : DomainException
{
    public ExternalServiceException(string serviceName, string message)
        : base($"Erro no serviço {serviceName}: {message}", "ERR-EXTERNAL-001")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"Erro no serviço {serviceName}: {message}", "ERR-EXTERNAL-001")
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}