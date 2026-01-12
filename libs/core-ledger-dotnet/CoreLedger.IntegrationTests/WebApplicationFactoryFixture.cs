using CoreLedger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace CoreLedger.IntegrationTests;

/// <summary>
///     WebApplicationFactory fixture for integration tests with Testcontainers Postgres.
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("core_ledger_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        HttpClient?.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Add DbContext using the Testcontainers connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Ensure the database is created and migrations are applied
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        });
    }
}