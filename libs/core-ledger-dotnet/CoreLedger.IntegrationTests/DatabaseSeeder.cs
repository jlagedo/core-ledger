using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.IntegrationTests;

/// <summary>
///     Seeds test data into the integration test database.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if already seeded
        if (await context.Accounts.AnyAsync()) return;

        // Seed account types
        var accountTypes = new[]
        {
            AccountType.Create("Assets"),
            AccountType.Create("Liabilities"),
            AccountType.Create("Equity"),
            AccountType.Create("Revenue"),
            AccountType.Create("Expenses"),
            AccountType.Create("Cash")
        };

        await context.AccountTypes.AddRangeAsync(accountTypes);
        await context.SaveChangesAsync();

        // Reload to get database-generated values
        var cashType = await context.AccountTypes.FirstAsync(at => at.Description == "Cash");
        var assetType = await context.AccountTypes.FirstAsync(at => at.Description == "Assets");
        var expenseType = await context.AccountTypes.FirstAsync(at => at.Description == "Expenses");

        // Seed accounts
        var accounts = new List<Account>();

        // Create 100 test accounts with variety
        for (var i = 0; i < 100; i++)
        {
            var typeToUse = i % 3 == 0 ? cashType.Id : i % 3 == 1 ? assetType.Id : expenseType.Id;
            var status = i % 10 == 0 ? AccountStatus.Inactive : AccountStatus.Active;
            var normalBalance = i % 2 == 0 ? NormalBalance.Debit : NormalBalance.Credit;

            var account = Account.Create(
                1000 + i,
                $"Test Account {i:D3}",
                typeToUse,
                status,
                normalBalance,
                "test|seeder"
            );

            accounts.Add(account);
        }

        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();
    }
}