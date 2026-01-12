using System.Net.Http.Headers;
using System.Text.Json;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
///     Service for interacting with Auth0 APIs.
/// </summary>
public class Auth0Service : IAuth0Service
{
    private readonly string _domain;
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0Service> _logger;
    private readonly bool _useMockAuth;

    public Auth0Service(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<Auth0Service> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _useMockAuth = configuration.GetValue<bool>("Auth:UseMock");

        // Only require Auth0:Domain when not using mock auth
        if (!_useMockAuth)
        {
            _domain = configuration["Auth0:Domain"]
                      ?? throw new InvalidOperationException("Auth0:Domain configuration is missing");

            // Ensure domain has proper format
            if (!_domain.StartsWith("https://")) _domain = $"https://{_domain}";
            if (_domain.EndsWith("/")) _domain = _domain.TrimEnd('/');
        }
        else
        {
            _domain = string.Empty;
        }
    }

    public async Task<Auth0UserProfile> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        // Return mock user data when in mock auth mode
        if (_useMockAuth)
        {
            _logger.LogDebug("Returning mock user profile (mock auth mode)");
            return new Auth0UserProfile(
                Sub: "mock|admin-001",
                Email: "admin@coreledger.local",
                Name: "Mock Admin User");
        }

        try
        {
            var requestUri = $"{_domain}/userinfo";

            _logger.LogInformation("Fetching user profile from Auth0 /userinfo endpoint");

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Auth0 /userinfo returned {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);

                throw new ExternalServiceException(
                    "Auth0",
                    $"Failed to fetch user profile: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<Auth0UserInfoResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (userInfo == null || string.IsNullOrEmpty(userInfo.Sub))
                throw new ExternalServiceException(
                    "Auth0",
                    "Invalid response from /userinfo endpoint");

            _logger.LogInformation(
                "Successfully retrieved Auth0 profile for user: {Sub}",
                userInfo.Sub);

            return new Auth0UserProfile(
                userInfo.Sub,
                userInfo.Email,
                userInfo.Name);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling Auth0 /userinfo");
            throw new ExternalServiceException("Auth0", "Network error occurred", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Auth0 /userinfo request timed out");
            throw new ExternalServiceException("Auth0", "Request timed out", ex);
        }
        catch (ExternalServiceException)
        {
            throw; // Re-throw our own exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Auth0 /userinfo");
            throw new ExternalServiceException("Auth0", "Unexpected error occurred", ex);
        }
    }

    /// <summary>
    ///     Internal DTO for Auth0 /userinfo response deserialization.
    /// </summary>
    private class Auth0UserInfoResponse
    {
        public string Sub { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}