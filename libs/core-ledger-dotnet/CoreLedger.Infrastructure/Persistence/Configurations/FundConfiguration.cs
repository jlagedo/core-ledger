using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy support

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     EF Core configuration for Fund entity.
/// </summary>
public class FundConfiguration : IEntityTypeConfiguration<Fund>
{
    public void Configure(EntityTypeBuilder<Fund> builder)
    {
        builder.ToTable("funds");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.BaseCurrency)
            .HasColumnName("base_currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(f => f.InceptionDate)
            .HasColumnName("inception_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(f => f.ValuationFrequency)
            .HasColumnName("valuation_frequency")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(f => f.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.HasIndex(f => f.Code)
            .IsUnique();

        builder.HasIndex(f => f.Name)
            .IsUnique();
    }
}