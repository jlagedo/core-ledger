using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for Instituicao entity.
/// </summary>
public class InstituicaoConfiguration : IEntityTypeConfiguration<Instituicao>
{
    public void Configure(EntityTypeBuilder<Instituicao> builder)
    {
        builder.ToTable("instituicao", "cadastros");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Value Object: CNPJ
        builder.Property(i => i.Cnpj)
            .HasColumnName("cnpj")
            .HasMaxLength(14)
            .HasConversion(
                v => v.Valor,
                v => CNPJ.Criar(v))
            .IsRequired();

        builder.Property(i => i.RazaoSocial)
            .HasColumnName("razao_social")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.NomeFantasia)
            .HasColumnName("nome_fantasia")
            .HasMaxLength(100);

        builder.Property(i => i.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        // Audit fields
        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(i => i.Cnpj)
            .IsUnique()
            .HasDatabaseName("ix_instituicao_cnpj");

        // Relationships
        builder.HasMany(i => i.Vinculos)
            .WithOne(v => v.Instituicao)
            .HasForeignKey(v => v.InstituicaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
