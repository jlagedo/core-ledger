using CoreLedger.Application;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Worker.Configuration;
using CoreLedger.Worker.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

// Build configuration to read Serilog settings before creating logger
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", true,
        true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "CoreLedgerWorker")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Core Ledger Worker");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog(Log.Logger);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure options
    builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
    builder.Services.Configure<TestConnectionOptions>(builder.Configuration.GetSection("TestConnection"));
    builder.Services.Configure<OutboxProcessorOptions>(builder.Configuration.GetSection("OutboxProcessor"));
    builder.Services.Configure<QueueNamesOptions>(builder.Configuration.GetSection("QueueNames"));
    builder.Services.Configure<WorkerHttpClientOptions>(builder.Configuration.GetSection("WorkerHttpClient"));

    // Configure health checks
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var rabbitMqOptions = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>()
        ?? throw new InvalidOperationException("RabbitMQ configuration not found");

    // Create shared RabbitMQ connection for health check (singleton pattern per RabbitMQ best practices)
    builder.Services.AddSingleton<Task<RabbitMQ.Client.IConnection>>(sp =>
    {
        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = rabbitMqOptions.Hostname,
            Port = int.Parse(rabbitMqOptions.Port),
            UserName = rabbitMqOptions.Username,
            Password = rabbitMqOptions.Password
        };
        return factory.CreateConnectionAsync();
    });

    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"),
            name: "database",
            tags: ["db", "sql", "postgres"])
        .AddRabbitMQ(sp => sp.GetRequiredService<Task<RabbitMQ.Client.IConnection>>(),
            name: "rabbitmq",
            tags: ["messaging", "rabbitmq"])
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["self"]);

    // Configure HttpClient for Worker -> API communication
    var workerHttpClientOptions = builder.Configuration.GetSection("WorkerHttpClient")
        .Get<WorkerHttpClientOptions>() ?? new WorkerHttpClientOptions();

    builder.Services.AddHttpClient("WorkerHttpClient", client =>
    {
        client.BaseAddress = new Uri(workerHttpClientOptions.ApiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(workerHttpClientOptions.TimeoutSeconds);
        client.DefaultRequestHeaders.Add("User-Agent", workerHttpClientOptions.UserAgent);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {workerHttpClientOptions.MockJwtToken}");
    });

    builder.Services.AddHostedService<B3ImportConsumer>();
    builder.Services.AddHostedService<TestConnectionConsumer>();
    builder.Services.AddHostedService<TransactionOutboxProcessor>();
    builder.Services.AddHostedService<TransactionProcessingConsumer>();

    var app = builder.Build();

    // Map health check endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("messaging")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("self")
    });

    Log.Information("Core Ledger Worker started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}