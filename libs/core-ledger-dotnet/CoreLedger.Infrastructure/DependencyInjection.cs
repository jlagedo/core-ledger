using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Infrastructure.Persistence;
using CoreLedger.Infrastructure.Services;
using CoreLedger.Infrastructure.Services.QueryServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLedger.Infrastructure;

/// <summary>
///     Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.Configure<HttpClientOptions>(configuration.GetSection("HttpClient"));
        services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
        services.Configure<B3ImportOptions>(configuration.GetSection("B3Import"));

        // Get database options for DbContext configuration
        var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .EnableRetryOnFailure(
                        databaseOptions.MaxRetryCount,
                        TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                        null)
                    .CommandTimeout(databaseOptions.CommandTimeoutSeconds)
                    .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Register IApplicationDbContext for handlers
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Query Services for complex RFC-8040 filtering operations
        services.AddScoped<IAccountQueryService, AccountQueryService>();
        services.AddScoped<IFundQueryService, FundQueryService>();
        services.AddScoped<ISecurityQueryService, SecurityQueryService>();
        services.AddScoped<ICoreJobQueryService, CoreJobQueryService>();
        services.AddScoped<ITransactionQueryService, TransactionQueryService>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<ICalendarioQueryService, CalendarioQueryService>();
        services.AddScoped<IIndexadorQueryService, IndexadorQueryService>();
        services.AddScoped<IHistoricoIndexadorQueryService, HistoricoIndexadorQueryService>();

        // Query Services for Cadastros module (Fundo, Instituicao)
        services.AddScoped<IFundoQueryService, FundoQueryService>();
        services.AddScoped<IInstituicaoQueryService, InstituicaoQueryService>();

        // Get HTTP client options for Auth0 service configuration
        var httpClientOptions =
            configuration.GetSection("HttpClient").Get<HttpClientOptions>() ?? new HttpClientOptions();

        // HttpClient for Auth0 API calls
        services.AddHttpClient<IAuth0Service, Auth0Service>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(httpClientOptions.Auth0TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", httpClientOptions.UserAgent);
        });

        // User management service
        services.AddScoped<IUserService, UserService>();

        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
        services.AddScoped<IB3ImportProcessor, B3ImportProcessor>();

        return services;
    }
}