using Microsoft.AspNetCore.SignalR;

namespace CoreLedger.API.Hubs;

/// <summary>
/// Custom SignalR user ID provider that uses the 'sub' claim from JWT tokens.
/// This enables SignalR to send notifications to specific users based on their Auth0 subject ID.
/// </summary>
public class SubUserIdProvider : IUserIdProvider
{
    /// <summary>
    /// Gets the user identifier from the 'sub' claim in the JWT token.
    /// </summary>
    /// <param name="connection">The hub connection context containing user claims.</param>
    /// <returns>The value of the 'sub' claim, or null if not found.</returns>
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("sub")?.Value;
    }
}
