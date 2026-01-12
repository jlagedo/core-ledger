using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for Transaction entity.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.FundId)
            .HasColumnName("fund_id")
            .IsRequired();

        builder.Property(t => t.SecurityId)
            .HasColumnName("security_id");

        builder.Property(t => t.TransactionSubTypeId)
            .HasColumnName("transaction_subtype_id")
            .IsRequired();

        builder.Property(t => t.TradeDate)
            .HasColumnName("trade_date")
            .IsRequired();

        builder.Property(t => t.SettleDate)
            .HasColumnName("settle_date")
            .IsRequired();

        builder.Property(t => t.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.Price)
            .HasColumnName("price")
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(t => t.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        // Foreign key relationships
        builder.HasOne(t => t.Fund)
            .WithMany()
            .HasForeignKey(t => t.FundId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Security)
            .WithMany()
            .HasForeignKey(t => t.SecurityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.TransactionSubType)
            .WithMany()
            .HasForeignKey(t => t.TransactionSubTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(t => t.FundId)
            .HasDatabaseName("ix_transactions_fund_id");

        builder.HasIndex(t => t.SecurityId)
            .HasDatabaseName("ix_transactions_security_id");

        builder.HasIndex(t => t.TransactionSubTypeId)
            .HasDatabaseName("ix_transactions_transaction_subtype_id");

        builder.HasIndex(t => t.StatusId)
            .HasDatabaseName("ix_transactions_status_id");

        builder.HasIndex(t => t.TradeDate)
            .HasDatabaseName("ix_transactions_trade_date");

        builder.HasIndex(t => t.SettleDate)
            .HasDatabaseName("ix_transactions_settle_date");

        // Composite index for common query pattern
        builder.HasIndex(t => new { t.FundId, t.TradeDate })
            .HasDatabaseName("ix_transactions_fund_trade_date");
    }
}