using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for TransactionStatus entity.
/// </summary>
public class TransactionStatusConfiguration : IEntityTypeConfiguration<TransactionStatus>
{
    public void Configure(EntityTypeBuilder<TransactionStatus> builder)
    {
        builder.ToTable("transaction_statuses");

        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ts => ts.ShortDescription)
            .HasColumnName("short_description")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ts => ts.LongDescription)
            .HasColumnName("long_description")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ts => ts.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ts => ts.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(ts => ts.ShortDescription)
            .HasDatabaseName("ix_transaction_statuses_short_description");
    }
}