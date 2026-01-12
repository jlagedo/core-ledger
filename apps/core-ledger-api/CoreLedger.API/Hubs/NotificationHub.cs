using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoreLedger.API.Hubs;

/// <summary>
/// SignalR hub for real-time notifications to connected clients.
/// Secured with JWT Bearer authentication.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation(
            "SignalR client connected - ConnectionId: {ConnectionId}, UserId: {UserId}",
            Context.ConnectionId, userId ?? "anonymous");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;

        if (exception != null)
        {
            _logger.LogWarning(exception,
                "SignalR client disconnected with error - ConnectionId: {ConnectionId}, UserId: {UserId}",
                Context.ConnectionId, userId ?? "anonymous");
        }
        else
        {
            _logger.LogInformation(
                "SignalR client disconnected - ConnectionId: {ConnectionId}, UserId: {UserId}",
                Context.ConnectionId, userId ?? "anonymous");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
