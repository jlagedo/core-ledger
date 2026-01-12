namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Interface for processing B3 import jobs.
/// </summary>
public interface IB3ImportProcessor
{
    /// <summary>
    ///     Processes a B3 import job by reading from b3_instruments_enriched and creating/updating securities.
    /// </summary>
    /// <param name="coreJobId">The CoreJob ID to process</param>
    /// <param name="referenceId">Reference ID for the job</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessAsync(int coreJobId, string referenceId, CancellationToken cancellationToken = default);
}