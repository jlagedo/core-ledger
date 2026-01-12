using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for TransactionType entity.
/// </summary>
public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("transaction_types");

        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(tt => tt.ShortDescription)
            .HasColumnName("short_description")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tt => tt.LongDescription)
            .HasColumnName("long_description")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tt => tt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(tt => tt.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(tt => tt.ShortDescription)
            .HasDatabaseName("ix_transaction_types_short_description");
    }
}