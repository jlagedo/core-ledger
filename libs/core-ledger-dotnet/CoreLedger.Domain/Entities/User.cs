using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     User domain entity representing authenticated users from external identity providers.
/// </summary>
public class User : BaseEntity
{
    private User()
    {
    }

    /// <summary>
    ///     The unique external user ID from Auth0 or other providers (the sub claim).
    /// </summary>
    public string AuthProviderId { get; private set; } = string.Empty;

    /// <summary>
    ///     Identity provider name (e.g., auth0, google-oauth2, github).
    /// </summary>
    public string Provider { get; private set; } = string.Empty;

    /// <summary>
    ///     Optional email returned by the provider.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    ///     Optional display name.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    ///     Timestamp of the user's most recent login.
    /// </summary>
    public DateTime LastLoginAt { get; private set; }

    /// <summary>
    ///     Identifier of the user who created this user record.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Factory method to create a new User with validation.
    /// </summary>
    public static User Create(
        string authProviderId,
        string provider,
        string createdByUserId,
        string? email = null,
        string? name = null)
    {
        ValidateAuthProviderId(authProviderId);
        ValidateProvider(provider);
        ValidateCreatedByUserId(createdByUserId);
        ValidateEmail(email);
        ValidateName(name);

        var now = DateTime.UtcNow;
        return new User
        {
            AuthProviderId = authProviderId.Trim(),
            Provider = provider.Trim().ToLowerInvariant(),
            Email = email?.Trim(),
            Name = name?.Trim(),
            LastLoginAt = now,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Updates the user's login timestamp and optional profile information.
    /// </summary>
    public void UpdateLoginInfo(string? email = null, string? name = null)
    {
        ValidateEmail(email);
        ValidateName(name);

        Email = email?.Trim();
        Name = name?.Trim();
        LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }

    /// <summary>
    ///     Updates the last login timestamp.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }

    private static void ValidateAuthProviderId(string authProviderId)
    {
        if (string.IsNullOrWhiteSpace(authProviderId))
            throw new DomainValidationException("Auth provider ID cannot be empty");

        if (authProviderId.Length > 255)
            throw new DomainValidationException("Auth provider ID cannot exceed 255 characters");
    }

    private static void ValidateProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new DomainValidationException("Provider cannot be empty");

        if (provider.Length > 50)
            throw new DomainValidationException("Provider cannot exceed 50 characters");
    }

    private static void ValidateEmail(string? email)
    {
        if (email != null && email.Length > 255)
            throw new DomainValidationException("Email cannot exceed 255 characters");
    }

    private static void ValidateName(string? name)
    {
        if (name != null && name.Length > 200)
            throw new DomainValidationException("Name cannot exceed 200 characters");
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new DomainValidationException("CreatedByUserId cannot be empty");
    }
}