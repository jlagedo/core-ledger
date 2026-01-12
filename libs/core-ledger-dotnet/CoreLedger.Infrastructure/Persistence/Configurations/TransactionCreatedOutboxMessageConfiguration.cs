using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework configuration for TransactionCreatedOutboxMessage entity.
/// </summary>
public class TransactionCreatedOutboxMessageConfiguration : IEntityTypeConfiguration<TransactionCreatedOutboxMessage>
{
    public void Configure(EntityTypeBuilder<TransactionCreatedOutboxMessage> builder)
    {
        builder.ToTable("transaction_created_outbox_message");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(m => m.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasColumnName("payload")
            .IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasDefaultValue(OutboxMessageStatus.Pending)
            .HasConversion<short>();

        builder.Property(m => m.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.LastError)
            .HasColumnName("last_error");

        builder.Property(m => m.PublishedOn)
            .HasColumnName("published_on");

        // Index on status for efficient polling of pending messages
        builder.HasIndex(m => m.Status);

        // Index on created_on for time-based queries
        builder.HasIndex(m => m.OccurredOn);
    }
}
