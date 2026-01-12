using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Service for managing user lifecycle and synchronization with Auth0.
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Ensures user exists in database, creating or updating from Auth0 if needed.
    /// </summary>
    /// <param name="authProviderId">The Auth0 subject claim (sub)</param>
    /// <param name="provider">The identity provider (e.g., "auth0")</param>
    /// <param name="accessToken">The user's access token for calling Auth0 /userinfo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user profile</returns>
    /// <exception cref="ExternalServiceException">When Auth0 API call fails</exception>
    Task<UserDto> EnsureUserExistsAsync(
        string authProviderId,
        string provider,
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets user by internal database ID.
    /// </summary>
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets user by Auth Provider ID and Provider.
    /// </summary>
    Task<UserDto?> GetByAuthProviderIdAsync(
        string authProviderId,
        string provider,
        CancellationToken cancellationToken = default);
}