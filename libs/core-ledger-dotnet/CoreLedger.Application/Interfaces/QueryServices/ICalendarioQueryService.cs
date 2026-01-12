using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for complex Calendario operations including pagination, filtering, and business day calculations.
/// </summary>
public interface ICalendarioQueryService
{
    /// <summary>
    ///     Gets calendarios with pagination, sorting, and filtering support.
    ///     Supports multiple simultaneous filters matching the frontend Angular application.
    /// </summary>
    Task<(IReadOnlyList<Calendario> Calendarios, int TotalCount)> GetWithQueryAsync(
        CalendarioQueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a calendar entry by date and market location.
    /// </summary>
    Task<Calendario?> GetByDataAndPracaAsync(
        DateOnly data,
        Praca praca,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a date is a business day for a specific market location.
    /// </summary>
    Task<bool> IsDiaUtilAsync(
        DateOnly data,
        Praca praca,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the next business day after the specified date for a market location.
    ///     Throws InvalidOperationException if no business day is found within 30 days.
    /// </summary>
    Task<DateOnly> GetProximoDiaUtilAsync(
        DateOnly data,
        Praca praca,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Calculates D+X business days from a starting date for a market location.
    ///     D+0 returns the same day if it's a business day, otherwise the next business day.
    ///     Throws ArgumentException if dias is negative.
    ///     Throws InvalidOperationException if insufficient business days found within 365 days.
    /// </summary>
    Task<DateOnly> CalcularDMaisAsync(
        DateOnly data,
        int dias,
        Praca praca,
        CancellationToken cancellationToken = default);
}
