using System.IdentityModel.Tokens.Jwt;
using CoreLedger.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CoreLedger.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication configured for Auth0.
    /// </summary>
    public static IServiceCollection AddAuth0Authentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Auth0 options
        services.Configure<Auth0Options>(configuration.GetSection("Auth0"));

        var auth0Options = configuration.GetSection("Auth0").Get<Auth0Options>()
                           ?? throw new InvalidOperationException("Auth0 configuration is missing");

        // Disable legacy WS-Fed claim mappings
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var domain = auth0Options.Domain
                     ?? throw new InvalidOperationException("Auth0:Domain configuration is missing");
        var audience = auth0Options.Audience
                       ?? throw new InvalidOperationException("Auth0:Audience configuration is missing");

        if (!domain.StartsWith("https://")) domain = $"https://{domain}";
        if (!domain.EndsWith("/")) domain = $"{domain}/";

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = domain,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(auth0Options.ClockSkewMinutes),

                    NameClaimType = "sub",
                    RoleClaimType = "permissions"
                };

                options.MapInboundClaims = false;

                // Configure SignalR WebSocket authentication
                options.ConfigureSignalRAuthentication();
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds simplified development authentication with comprehensive debug logging.
    /// </summary>
    public static IServiceCollection AddDevelopmentAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Enable detailed logging for troubleshooting
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,

                    // Preserve claim types (don't map 'sub' to ClaimTypes.NameIdentifier)
                    NameClaimType = "sub",
                    RoleClaimType = "permissions"
                };

                // Don't map inbound claims (preserve 'sub' as-is)
                options.MapInboundClaims = false;

                // Debug logging events
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerAuthentication");

                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        var hasToken = !string.IsNullOrEmpty(authHeader);

                        logger.LogDebug(
                            "OnMessageReceived - Path: {Path}, Method: {Method}, HasAuthHeader: {HasAuth}, AuthHeaderPrefix: {Prefix}",
                            context.Request.Path,
                            context.Request.Method,
                            hasToken,
                            hasToken ? authHeader?.Split(' ').FirstOrDefault() : "none");

                        if (hasToken && authHeader!.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            var token = authHeader["Bearer ".Length..];
                            // Log first and last 10 chars only for security
                            var tokenPreview = token.Length > 20
                                ? $"{token[..10]}...{token[^10..]}"
                                : "[short token]";
                            logger.LogDebug("Token preview: {TokenPreview}, Length: {Length}",
                                tokenPreview, token.Length);
                        }

                        // Extract token from query string for SignalR WebSocket connections
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                            logger.LogDebug("SignalR WebSocket token extracted from query string");
                        }

                        return Task.CompletedTask;
                    },

                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerAuthentication");

                        var claims = context.Principal?.Claims
                            .Select(c => $"{c.Type}={c.Value}")
                            .ToList() ?? [];

                        logger.LogDebug(
                            "OnTokenValidated - Success! Claims count: {Count}, Claims: {Claims}",
                            claims.Count,
                            string.Join(", ", claims));

                        var subClaim = context.Principal?.FindFirst("sub")?.Value;
                        var nameClaim = context.Principal?.Identity?.Name;

                        logger.LogDebug(
                            "Identity - Sub: {Sub}, Name: {Name}, IsAuthenticated: {IsAuth}",
                            subClaim ?? "null",
                            nameClaim ?? "null",
                            context.Principal?.Identity?.IsAuthenticated ?? false);

                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerAuthentication");

                        logger.LogWarning(
                            context.Exception,
                            "OnAuthenticationFailed - Exception: {ExceptionType}, Message: {Message}",
                            context.Exception.GetType().Name,
                            context.Exception.Message);

                        if (context.Exception.InnerException != null)
                        {
                            logger.LogWarning(
                                "InnerException - Type: {Type}, Message: {Message}",
                                context.Exception.InnerException.GetType().Name,
                                context.Exception.InnerException.Message);
                        }

                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerAuthentication");

                        logger.LogDebug(
                            "OnChallenge - Error: {Error}, ErrorDescription: {Description}, AuthenticateFailure: {Failure}",
                            context.Error ?? "none",
                            context.ErrorDescription ?? "none",
                            context.AuthenticateFailure?.Message ?? "none");

                        return Task.CompletedTask;
                    },

                    OnForbidden = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerAuthentication");

                        var user = context.Principal?.Identity?.Name ?? "anonymous";
                        var claims = context.Principal?.Claims
                            .Select(c => $"{c.Type}={c.Value}")
                            .ToList() ?? [];

                        logger.LogWarning(
                            "OnForbidden - User: {User}, Path: {Path}, Claims: {Claims}",
                            user,
                            context.Request.Path,
                            string.Join(", ", claims));

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}