using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Configuração Entity Framework para a entidade Account.
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.TypeId)
            .HasColumnName("type_id")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(a => a.NormalBalance)
            .HasColumnName("normal_balance")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(a => a.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.HasOne(a => a.Type)
            .WithMany()
            .HasForeignKey(a => a.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.Code)
            .IsUnique()
            .HasDatabaseName("ix_accounts_code");

        builder.HasIndex(a => a.TypeId)
            .HasDatabaseName("ix_accounts_type_id");
    }
}