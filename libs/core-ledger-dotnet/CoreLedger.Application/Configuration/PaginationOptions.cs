namespace CoreLedger.Application.Configuration;

/// <summary>
///     Pagination configuration options
/// </summary>
public class PaginationOptions
{
    /// <summary>
    ///     Default page size for paginated queries
    /// </summary>
    public int DefaultPageSize { get; set; } = 100;

    /// <summary>
    ///     Maximum allowed page size
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}