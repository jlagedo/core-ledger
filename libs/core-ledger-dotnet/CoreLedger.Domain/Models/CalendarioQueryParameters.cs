namespace CoreLedger.Domain.Models;

/// <summary>
///     Query parameters for Calendario list operations with comprehensive filtering support.
///     Supports multiple simultaneous filters matching the frontend Angular application.
/// </summary>
public class CalendarioQueryParameters
{
    private int _limit = 100;
    private int _offset;
    private string _sortDirection = "asc";

    /// <summary>
    ///     Maximum number of items to return (hard limit: 100, minimum: 1).
    ///     Automatically clamped to valid range [1, 100].
    /// </summary>
    public int Limit
    {
        get => _limit;
        set => _limit = value < 1 ? 100 : Math.Min(value, 100);
    }

    /// <summary>
    ///     Number of items to skip (for pagination).
    ///     Automatically clamped to minimum of 0.
    /// </summary>
    public int Offset
    {
        get => _offset;
        set => _offset = Math.Max(value, 0);
    }

    /// <summary>
    ///     Field to sort by. Only whitelisted fields are used.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    ///     Sort direction (asc or desc). Automatically normalized to lowercase.
    ///     Invalid values default to "asc".
    /// </summary>
    public string SortDirection
    {
        get => _sortDirection;
        set => _sortDirection = ValidateSortDirection(value);
    }

    // Individual filter parameters

    /// <summary>
    ///     Text search on description field (case-insensitive substring match).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    ///     Filter by Praca enum value (1=Nacional, 2=SaoPaulo, etc.).
    /// </summary>
    public int? Praca { get; set; }

    /// <summary>
    ///     Filter by TipoDia enum value (1=Util, 2=FeriadoNacional, etc.).
    /// </summary>
    public int? TipoDia { get; set; }

    /// <summary>
    ///     Filter by business day flag.
    /// </summary>
    public bool? DiaUtil { get; set; }

    /// <summary>
    ///     Filter by start date (inclusive). ISO date format (yyyy-MM-dd).
    /// </summary>
    public DateOnly? DataInicio { get; set; }

    /// <summary>
    ///     Filter by end date (inclusive). ISO date format (yyyy-MM-dd).
    /// </summary>
    public DateOnly? DataFim { get; set; }

    /// <summary>
    ///     Checks if any filter is active.
    /// </summary>
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) ||
        Praca.HasValue ||
        TipoDia.HasValue ||
        DiaUtil.HasValue ||
        DataInicio.HasValue ||
        DataFim.HasValue;

    private static string ValidateSortDirection(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "asc";

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "asc" => "asc",
            "desc" => "desc",
            _ => "asc"
        };
    }
}
