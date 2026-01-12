using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for TransactionSubType entity.
/// </summary>
public class TransactionSubTypeConfiguration : IEntityTypeConfiguration<TransactionSubType>
{
    public void Configure(EntityTypeBuilder<TransactionSubType> builder)
    {
        builder.ToTable("transaction_subtypes");

        builder.HasKey(tst => tst.Id);

        builder.Property(tst => tst.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(tst => tst.TypeId)
            .HasColumnName("type_id")
            .IsRequired();

        builder.Property(tst => tst.ShortDescription)
            .HasColumnName("short_description")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tst => tst.LongDescription)
            .HasColumnName("long_description")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tst => tst.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(tst => tst.UpdatedAt)
            .HasColumnName("updated_at");

        // Foreign key relationship
        builder.HasOne(tst => tst.Type)
            .WithMany()
            .HasForeignKey(tst => tst.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(tst => tst.TypeId)
            .HasDatabaseName("ix_transaction_subtypes_type_id");

        builder.HasIndex(tst => tst.ShortDescription)
            .HasDatabaseName("ix_transaction_subtypes_short_description");
    }
}