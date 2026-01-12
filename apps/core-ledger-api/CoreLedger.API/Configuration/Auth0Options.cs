namespace CoreLedger.API.Configuration;

/// <summary>
///     Auth0 JWT authentication configuration options
/// </summary>
public class Auth0Options
{
    /// <summary>
    ///     Auth0 domain
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    ///     Auth0 audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     Clock skew tolerance in minutes for JWT validation
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;
}