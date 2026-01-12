namespace CoreLedger.Domain.Models;

/// <summary>
///     Query parameters for Indexador list operations with comprehensive filtering support.
///     Supports multiple simultaneous filters matching the frontend Angular application.
/// </summary>
public class IndexadorQueryParameters
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
    ///     Text search on codigo and nome fields (case-insensitive substring match).
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    ///     Filter by IndexadorTipo enum value (0=Juros, 1=Inflacao, 2=Cambio, etc.).
    /// </summary>
    public int? Tipo { get; set; }

    /// <summary>
    ///     Filter by Periodicidade enum value (0=Diaria, 1=Mensal, 2=Anual).
    /// </summary>
    public int? Periodicidade { get; set; }

    /// <summary>
    ///     Filter by fonte (case-insensitive substring match).
    /// </summary>
    public string? Fonte { get; set; }

    /// <summary>
    ///     Filter by ativo flag.
    /// </summary>
    public bool? Ativo { get; set; }

    /// <summary>
    ///     Filter by importacao automatica flag.
    /// </summary>
    public bool? ImportacaoAutomatica { get; set; }

    /// <summary>
    ///     Checks if any filter is active.
    /// </summary>
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Filter) ||
        Tipo.HasValue ||
        Periodicidade.HasValue ||
        !string.IsNullOrWhiteSpace(Fonte) ||
        Ativo.HasValue ||
        ImportacaoAutomatica.HasValue;

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
