using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Infrastructure.Persistence;
using CoreLedger.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
///     Processes B3 import jobs by reading from b3_instruments_enriched and creating/updating securities.
/// </summary>
public class B3ImportProcessor : IB3ImportProcessor
{
    private readonly int _batchSize;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<B3ImportProcessor> _logger;

    public B3ImportProcessor(
        ILogger<B3ImportProcessor> logger,
        ApplicationDbContext dbContext,
        IOptions<B3ImportOptions> options)
    {
        _logger = logger;
        _dbContext = dbContext;
        _batchSize = options.Value.BatchSize;
    }

    /// <summary>
    ///     Processes a B3 import job by reading from b3_instruments_enriched and creating/updating securities.
    ///     Uses efficient database strategies: streaming/chunked processing, batch operations, single transaction.
    ///     Processes instruments in batches to handle large datasets without excessive memory consumption.
    /// </summary>
    public async Task ProcessAsync(int coreJobId, string referenceId, CancellationToken cancellationToken = default)
    {
        var coreJob = await _dbContext.CoreJobs.FindAsync([coreJobId], cancellationToken);
        if (coreJob == null)
        {
            _logger.LogError("CoreJob {CoreJobId} not found", coreJobId);
            throw new InvalidOperationException($"CoreJob {coreJobId} not found");
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var startTime = DateTime.UtcNow;
                _logger.LogInformation("========================================");
                _logger.LogInformation("Starting B3 import for CoreJob {CoreJobId}, ReferenceId {ReferenceId}",
                    coreJobId, referenceId);
                _logger.LogInformation("========================================");

                coreJob.UpdateStatus(JobStatus.Running, DateTime.UtcNow);
                // Save changes within the transaction
                _logger.LogInformation("CoreJob status updated to Running");

                _logger.LogInformation("Querying total instrument count from b3_instruments_enriched...");
                var totalCount = await _dbContext.Database
                    .SqlQuery<int>(
                        $"SELECT COUNT(*) AS \"Value\" FROM b3_instruments_enriched WHERE \"TckrSymb\" IS NOT NULL AND instrument_name IS NOT NULL")
                    .FirstOrDefaultAsync(cancellationToken);

                _logger.LogInformation(
                    "Total instruments to process: {TotalCount} | Batch size: {BatchSize} | Estimated batches: {EstimatedBatches}",
                    totalCount, _batchSize, (int)Math.Ceiling((double)totalCount / _batchSize));

                var processedCount = 0;
                var createdCount = 0;
                var updatedCount = 0;
                var offset = 0;
                var batchNumber = 0;

                while (offset < totalCount)
                {
                    batchNumber++;
                    var batchStartTime = DateTime.UtcNow;
                    var progressPercent = totalCount > 0 ? (int)((double)offset / totalCount * 100) : 0;

                    _logger.LogInformation("----------------------------------------");
                    _logger.LogInformation(
                        "Batch {BatchNumber} | Progress: {Progress}% | Offset: {Offset}/{TotalCount}",
                        batchNumber, progressPercent, offset, totalCount);

                    var instruments = await _dbContext.Database
                        .SqlQuery<B3InstrumentsEnriched>(
                            $"SELECT instrument_name AS \"InstrumentName\", \"TckrSymb\" AS \"Ticker\", \"ISIN\" AS \"Isin\", securitytypeid AS \"SecurityTypeId\", \"TradgCcy\" AS \"Currency\" FROM b3_instruments_enriched WHERE \"TckrSymb\" IS NOT NULL AND instrument_name IS NOT NULL ORDER BY \"TckrSymb\" LIMIT {_batchSize} OFFSET {offset}")
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("Retrieved {Count} instruments from database", instruments.Count);

                    if (!instruments.Any())
                    {
                        _logger.LogWarning("No more instruments found, breaking loop");
                        break;
                    }

                    var tickers = instruments.Select(i => i.Ticker.ToUpperInvariant()).ToHashSet();
                    _logger.LogInformation("Loading existing securities for {TickerCount} unique tickers...",
                        tickers.Count);

                    var existingSecurities = await _dbContext.Set<Security>()
                        .Where(s => tickers.Contains(s.Ticker))
                        .ToDictionaryAsync(s => s.Ticker, cancellationToken);

                    _logger.LogInformation("Found {ExistingCount} existing securities in database",
                        existingSecurities.Count);

                    var newSecurities = new List<Security>();
                    var batchCreated = 0;
                    var batchUpdated = 0;
                    var batchFailed = 0;

                    foreach (var instrument in instruments)
                        try
                        {
                            var securityType = (SecurityType)instrument.SecurityTypeId;
                            var tickerKey = instrument.Ticker.ToUpperInvariant();

                            if (!existingSecurities.TryGetValue(tickerKey, out var existingSecurity))
                            {
                                var newSecurity = Security.Create(
                                    instrument.InstrumentName,
                                    instrument.Ticker,
                                    instrument.Isin,
                                    securityType,
                                    instrument.Currency,
                                    "system|b3-import");

                                newSecurities.Add(newSecurity);
                                batchCreated++;
                                createdCount++;

                                if (batchCreated <= 3)
                                    _logger.LogDebug("Creating new security: {Ticker} - {Name}", instrument.Ticker,
                                        instrument.InstrumentName);
                            }
                            else
                            {
                                existingSecurity.Update(
                                    instrument.InstrumentName,
                                    instrument.Ticker,
                                    instrument.Isin,
                                    securityType,
                                    instrument.Currency);

                                batchUpdated++;
                                updatedCount++;

                                if (batchUpdated <= 3)
                                    _logger.LogDebug("Updating security: {Ticker} - {Name}", instrument.Ticker,
                                        instrument.InstrumentName);
                            }

                            processedCount++;
                        }
                        catch (Exception ex)
                        {
                            batchFailed++;
                            _logger.LogWarning(ex, "Failed to process instrument {Ticker}", instrument.Ticker);
                        }

                    _logger.LogInformation(
                        "Batch {BatchNumber} summary: Created={Created}, Updated={Updated}, Failed={Failed}",
                        batchNumber, batchCreated, batchUpdated, batchFailed);

                    if (newSecurities.Any())
                    {
                        _logger.LogInformation("Adding {Count} new securities to context...", newSecurities.Count);
                        await _dbContext.Set<Security>().AddRangeAsync(newSecurities, cancellationToken);
                    }

                    _logger.LogInformation("Saving changes to database...");
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    var batchDuration = DateTime.UtcNow - batchStartTime;
                    _logger.LogInformation("Batch {BatchNumber} committed successfully in {Duration}ms",
                        batchNumber, (int)batchDuration.TotalMilliseconds);

                    offset += _batchSize;
                }

                _logger.LogInformation("Finalizing B3 import process...");
                coreJob.UpdateStatus(JobStatus.Complete, finishedDate: DateTime.UtcNow);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("CoreJob status updated to Complete");

                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Transaction committed successfully");

                var totalDuration = DateTime.UtcNow - startTime;
                var avgBatchTime = batchNumber > 0 ? totalDuration.TotalMilliseconds / batchNumber : 0;

                _logger.LogInformation("========================================");
                _logger.LogInformation("B3 IMPORT COMPLETED SUCCESSFULLY");
                _logger.LogInformation("========================================");
                _logger.LogInformation("CoreJob ID: {CoreJobId}", coreJobId);
                _logger.LogInformation("Reference ID: {ReferenceId}", referenceId);
                _logger.LogInformation("Total Duration: {Duration}ms ({Minutes}min {Seconds}s)",
                    (int)totalDuration.TotalMilliseconds,
                    (int)totalDuration.TotalMinutes,
                    totalDuration.Seconds);
                _logger.LogInformation("Total Batches: {Batches}", batchNumber);
                _logger.LogInformation("Avg Batch Time: {AvgTime}ms", (int)avgBatchTime);
                _logger.LogInformation("Total Processed: {Processed}", processedCount);
                _logger.LogInformation("Total Created: {Created}", createdCount);
                _logger.LogInformation("Total Updated: {Updated}", updatedCount);
                _logger.LogInformation("Success Rate: {Rate}%",
                    totalCount > 0 ? (int)((double)processedCount / totalCount * 100) : 100);
                _logger.LogInformation("========================================");
            }
            catch (Exception ex)
            {
                _logger.LogError("========================================");
                _logger.LogError(ex, "B3 IMPORT FAILED for CoreJob {CoreJobId}", coreJobId);
                _logger.LogError("Error: {ErrorMessage}", ex.Message);
                _logger.LogError("========================================");

                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transaction rolled back");

                coreJob.UpdateStatus(JobStatus.Failed, finishedDate: DateTime.UtcNow);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("CoreJob status updated to Failed");

                throw;
            }
        });
    }
}