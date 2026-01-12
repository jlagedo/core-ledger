using CoreLedger.API.Configuration;
using CoreLedger.API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace CoreLedger.API.Extensions;

public static class SignalRExtensions
{
    /// <summary>
    /// Adds SignalR with Redis backplane for horizontal scaling.
    /// </summary>
    public static IServiceCollection AddSignalRWithRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>()
                           ?? new RedisOptions();

        services.AddSignalR()
            .AddStackExchangeRedis(redisOptions.ConnectionString, options =>
            {
                options.Configuration.ChannelPrefix =
                    new RedisChannel(redisOptions.ChannelPrefix, RedisChannel.PatternMode.Literal);
            });

        // Register custom user ID provider to use 'sub' claim from JWT tokens
        services.AddSingleton<IUserIdProvider, SubUserIdProvider>();

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer authentication events for SignalR WebSocket connections.
    /// SignalR sends the token as a query parameter since WebSocket doesn't support headers.
    /// </summary>
    public static void ConfigureSignalRAuthentication(this JwtBearerOptions options)
    {
        var existingOnMessageReceived = options.Events?.OnMessageReceived;

        options.Events ??= new JwtBearerEvents();
        options.Events.OnMessageReceived = async context =>
        {
            // Call existing handler if present
            if (existingOnMessageReceived != null)
            {
                await existingOnMessageReceived(context);
            }

            // Extract token from query string for SignalR WebSocket connections
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
        };
    }

    /// <summary>
    /// Maps SignalR hub endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapSignalRHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationHub>("/hubs/notifications");
        return endpoints;
    }
}
