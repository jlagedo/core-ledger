namespace CoreLedger.Application.Configuration;

/// <summary>
///     Static accessor for pagination defaults.
///     Initialized during application startup.
/// </summary>
public static class PaginationDefaults
{
    private static int _defaultPageSize = 100;
    private static int _maxPageSize = 100;

    /// <summary>
    ///     Default page size for paginated queries
    /// </summary>
    public static int DefaultPageSize
    {
        get => _defaultPageSize;
        set => _defaultPageSize = value > 0 ? value : 100;
    }

    /// <summary>
    ///     Maximum allowed page size
    /// </summary>
    public static int MaxPageSize
    {
        get => _maxPageSize;
        set => _maxPageSize = value > 0 ? value : 100;
    }

    /// <summary>
    ///     Initialize pagination defaults from configuration options
    /// </summary>
    public static void Initialize(PaginationOptions options)
    {
        DefaultPageSize = options.DefaultPageSize;
        MaxPageSize = options.MaxPageSize;
    }
}