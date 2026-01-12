using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     EF Core configuration for Security entity.
/// </summary>
public class SecurityConfiguration : IEntityTypeConfiguration<Security>
{
    public void Configure(EntityTypeBuilder<Security> builder)
    {
        builder.ToTable("securities");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Ticker)
            .HasColumnName("ticker")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Isin)
            .HasColumnName("isin")
            .HasMaxLength(12);

        builder.Property(s => s.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.DeactivatedAt)
            .HasColumnName("deactivated_at");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(s => s.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        // Unique index on ticker
        builder.HasIndex(s => s.Ticker)
            .IsUnique()
            .HasDatabaseName("ix_securities_ticker");

        // Non-unique index on type for filtering
        builder.HasIndex(s => s.Type)
            .HasDatabaseName("ix_securities_type");

        // Index on status for filtering active/inactive
        builder.HasIndex(s => s.Status)
            .HasDatabaseName("ix_securities_status");
    }
}