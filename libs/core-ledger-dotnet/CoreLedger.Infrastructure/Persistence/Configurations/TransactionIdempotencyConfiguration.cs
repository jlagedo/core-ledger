using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for TransactionIdempotency entity.
/// </summary>
public class TransactionIdempotencyConfiguration : IEntityTypeConfiguration<TransactionIdempotency>
{
    public void Configure(EntityTypeBuilder<TransactionIdempotency> builder)
    {
        builder.ToTable("transaction_idempotency");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(t => t.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.TransactionId)
            .HasColumnName("transaction_id");

        // Unique constraint on idempotency_key
        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique();

        // Optional foreign key to transactions table
        builder.HasOne<Transaction>()
            .WithMany()
            .HasForeignKey(t => t.TransactionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
