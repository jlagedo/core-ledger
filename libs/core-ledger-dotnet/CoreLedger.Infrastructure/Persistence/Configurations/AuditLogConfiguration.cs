using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Configuração Entity Framework para a entidade AuditLog.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(a => a.EntityName)
            .HasColumnName("entity_name")
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(a => a.EventType)
            .HasColumnName("event_type")
            .IsRequired();

        builder.Property(a => a.PerformedByUserId)
            .HasColumnName("performed_by_user_id");

        builder.Property(a => a.PerformedAt)
            .HasColumnName("performed_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.DataBefore)
            .HasColumnName("data_before")
            .HasColumnType("jsonb");

        builder.Property(a => a.DataAfter)
            .HasColumnName("data_after")
            .HasColumnType("jsonb");

        builder.Property(a => a.CorrelationId)
            .HasColumnName("correlation_id");

        builder.Property(a => a.RequestId)
            .HasColumnName("request_id");

        builder.Property(a => a.Source)
            .HasColumnName("source");
    }
}
