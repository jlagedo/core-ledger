using CoreLedger.API.Endpoints;
using CoreLedger.API.Endpoints.Cadastros;
using CoreLedger.API.Extensions;
using CoreLedger.API.Middleware;
using CoreLedger.Application;
using CoreLedger.Application.Configuration;
using CoreLedger.Infrastructure;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// Build configuration to read Serilog settings before creating logger
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
        true, true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "CoreLedgerApi")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Core Ledger API");

    var builder = WebApplication.CreateBuilder(args);
    
    // Use mock authentication in development (bypasses Auth0)
    var useMockAuth = builder.Configuration.GetValue<bool>("Auth:UseMock");

    builder.Host.UseSerilog();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure pagination options
    builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
    var paginationOptions = builder.Configuration.GetSection("Pagination").Get<PaginationOptions>() ??
                            new PaginationOptions();
    PaginationDefaults.Initialize(paginationOptions);

    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddSignalRWithRedis(builder.Configuration);

    if (useMockAuth)
    {
        Log.Warning("Starting with mock auth");
        builder.Services.AddDevelopmentAuthentication(builder.Configuration);
    }
    else
    {
        builder.Services.AddAuth0Authentication(builder.Configuration);
    }

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"))
        .AddCheck("self", () => HealthCheckResult.Healthy());

    var app = builder.Build();

    // HTTPS and HSTS only in production (development uses HTTP only)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseSecurityHeaders();

    // Authentication must come before correlation ID middleware to ensure user claims are available
    app.UseAuthentication();
    app.UseCorrelationId();

    // Serilog request logging must come BEFORE exception handler so it logs the correct status code
    // Pipeline: SerilogRequestLogging → GlobalExceptionHandler → Endpoints
    // When exception bubbles up: Endpoints → GlobalExceptionHandler (sets 400) → SerilogRequestLogging (logs 400)
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);

            // Add authenticated user information to request logs (only 'sub' claim available in access token)
            var userId = httpContext.User.FindFirst("sub")?.Value;
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;

            diagnosticContext.Set("UserId", userId ?? "anonymous");
            diagnosticContext.Set("IsAuthenticated", isAuthenticated);
        };
    });

    // Exception handler must be AFTER Serilog so correct status codes are logged
    app.UseGlobalExceptionHandler();

    if (app.Environment.IsDevelopment()) app.UseSwaggerDocumentation();

    app.UseAuthorization();

    // Map SignalR hubs
    app.MapSignalRHubs();

    // Minimal API endpoint registrations
    app.MapFundsEndpoints();
    app.MapAccountsEndpoints();
    app.MapTransactionsEndpoints();
    app.MapSecuritiesEndpoints();
    app.MapUsersEndpoints();
    app.MapJobsIngestionEndpoints();
    app.MapWorkerNotificationsEndpoints();
    app.MapAuditLogsEndpoints();
    app.MapCoreJobsEndpoints();
    app.MapAccountTypesEndpoints();
    app.MapTransactionTypesEndpoints();
    app.MapTransactionSubTypesEndpoints();
    app.MapTransactionStatusesEndpoints();
    app.MapSecurityTypesEndpoints();
    app.MapCalendarioEndpoints();
    app.MapIndexadoresEndpoints();
    app.MapHistoricosIndexadoresEndpoints();

    // Cadastros module endpoints
    app.MapFundosEndpoints();
    app.MapClassesEndpoints();
    app.MapTaxasEndpoints();
    app.MapPrazosEndpoints();
    app.MapVinculosEndpoints();
    app.MapInstituicoesEndpoints();
    app.MapClassificacoesAnbimaEndpoints();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
        if (addresses != null && addresses.Any())
        {
            Log.Information("Core Ledger API is now listening on:");
            foreach (var address in addresses) Log.Information("  → {Address}", address);
        }
    });

    Log.Information("Core Ledger API started successfully");


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
///     Partial Program class to expose entry point for integration tests.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program
{
}