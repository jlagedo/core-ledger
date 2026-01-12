using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     EF Core configuration for User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.AuthProviderId)
            .HasColumnName("auth_provider_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Provider)
            .HasColumnName("provider")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(200);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at")
            .IsRequired();

        builder.Property(u => u.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        // Unique constraint on AuthProviderId and Provider combination
        builder.HasIndex(u => new { u.AuthProviderId, u.Provider })
            .IsUnique();

        // Index on email for faster lookups (not unique as email can be null or shared)
        builder.HasIndex(u => u.Email);
    }
}