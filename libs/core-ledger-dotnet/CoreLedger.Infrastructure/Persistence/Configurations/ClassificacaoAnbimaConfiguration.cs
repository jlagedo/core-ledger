using CoreLedger.Domain.Cadastros.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for ClassificacaoAnbima entity.
/// </summary>
public class ClassificacaoAnbimaConfiguration : IEntityTypeConfiguration<ClassificacaoAnbima>
{
    public void Configure(EntityTypeBuilder<ClassificacaoAnbima> builder)
    {
        builder.ToTable("classificacao_anbima", "cadastros");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Nivel1)
            .HasColumnName("nivel1")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Nivel2)
            .HasColumnName("nivel2")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Nivel3)
            .HasColumnName("nivel3")
            .HasMaxLength(50);

        builder.Property(c => c.ClassificacaoCvm)
            .HasColumnName("classificacao_cvm")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.Descricao)
            .HasColumnName("descricao")
            .HasColumnType("text");

        builder.Property(c => c.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(c => c.OrdemExibicao)
            .HasColumnName("ordem_exibicao")
            .HasDefaultValue(0)
            .IsRequired();

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(c => c.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_classificacao_anbima_codigo");

        builder.HasIndex(c => c.ClassificacaoCvm)
            .HasDatabaseName("ix_classificacao_anbima_classificacao_cvm");

        builder.HasIndex(c => c.Nivel1)
            .HasDatabaseName("ix_classificacao_anbima_nivel1");

        builder.HasIndex(c => c.Ativo)
            .HasFilter("ativo = true")
            .HasDatabaseName("ix_classificacao_anbima_ativo");
    }
}
