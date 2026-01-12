using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Service for interacting with Auth0 Management API.
/// </summary>
public interface IAuth0Service
{
    /// <summary>
    ///     Retrieves user profile information from Auth0 /userinfo endpoint.
    /// </summary>
    /// <param name="accessToken">The user's access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile with email and name</returns>
    /// <exception cref="ExternalServiceException">When Auth0 API call fails</exception>
    Task<Auth0UserProfile> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}

/// <summary>
///     Auth0 user profile response.
/// </summary>
public record Auth0UserProfile(
    string Sub,
    string? Email,
    string? Name
);