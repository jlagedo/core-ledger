using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Configuração Entity Framework para a entidade AccountType.
/// </summary>
public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.ToTable("account_types");

        builder.HasKey(at => at.Id);

        builder.Property(at => at.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(at => at.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(at => at.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(at => at.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(at => at.Description)
            .IsUnique()
            .HasDatabaseName("ix_account_types_description");
    }
}