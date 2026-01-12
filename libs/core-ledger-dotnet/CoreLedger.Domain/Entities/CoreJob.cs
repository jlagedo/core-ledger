using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio CoreJob representando uma tarefa de fundo com rastreamento de status.
/// </summary>
public class CoreJob : BaseEntity
{
    private CoreJob()
    {
    }

    public string ReferenceId { get; private set; } = string.Empty;
    public JobStatus Status { get; private set; }
    public string JobDescription { get; private set; } = string.Empty;
    public DateTime CreationDate { get; private set; }
    public DateTime? RunningDate { get; private set; }
    public DateTime? FinishedDate { get; private set; }

    /// <summary>
    ///     Método factory para criar um novo CoreJob com validação.
    /// </summary>
    public static CoreJob Create(
        string referenceId,
        string jobDescription)
    {
        ValidateReferenceId(referenceId);
        ValidateJobDescription(jobDescription);

        return new CoreJob
        {
            ReferenceId = referenceId.Trim(),
            JobDescription = jobDescription.Trim(),
            Status = JobStatus.New,
            CreationDate = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza o status do job com datas opcionais de execução e conclusão.
    /// </summary>
    public void UpdateStatus(
        JobStatus status,
        DateTime? runningDate = null,
        DateTime? finishedDate = null)
    {
        ValidateStatusTransition(status, runningDate, finishedDate);

        Status = status;
        RunningDate = runningDate;
        FinishedDate = finishedDate;
        SetUpdated();
    }

    private static void ValidateReferenceId(string referenceId)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
            throw new DomainValidationException("ID de Referência não pode estar vazio");

        if (referenceId.Length > 50)
            throw new DomainValidationException("ID de Referência não pode exceder 50 caracteres");
    }

    private static void ValidateJobDescription(string jobDescription)
    {
        if (string.IsNullOrWhiteSpace(jobDescription))
            throw new DomainValidationException("Descrição do job não pode estar vazia");

        if (jobDescription.Length > 255)
            throw new DomainValidationException("Descrição do job não pode exceder 255 caracteres");
    }

    private static void ValidateStatusTransition(
        JobStatus status,
        DateTime? runningDate,
        DateTime? finishedDate)
    {
        if (status == JobStatus.Running && !runningDate.HasValue)
            throw new DomainValidationException("Data de execução deve ser definida quando o status é Executando");

        if ((status == JobStatus.Complete || status == JobStatus.Failed) && !finishedDate.HasValue)
            throw new DomainValidationException("Data de conclusão deve ser definida quando o status é Concluído ou Falhou");
    }
}