using CoreLedger.Domain.Cadastros.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoSubclasse entity.
/// </summary>
public class FundoSubclasseConfiguration : IEntityTypeConfiguration<FundoSubclasse>
{
    public void Configure(EntityTypeBuilder<FundoSubclasse> builder)
    {
        builder.ToTable("fundo_subclasse", "cadastros");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(s => s.ClasseId)
            .HasColumnName("classe_id")
            .IsRequired();

        builder.Property(s => s.CodigoSubclasse)
            .HasColumnName("codigo_subclasse")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.NomeSubclasse)
            .HasColumnName("nome_subclasse")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Serie)
            .HasColumnName("serie");

        builder.Property(s => s.ValorMinimoAplicacao)
            .HasColumnName("valor_minimo_aplicacao")
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.TaxaAdministracaoDiferenciada)
            .HasColumnName("taxa_administracao_diferenciada")
            .HasColumnType("decimal(8,4)");

        builder.Property(s => s.Ativa)
            .HasColumnName("ativa")
            .HasDefaultValue(true)
            .IsRequired();

        // Audit fields
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationship: FundoSubclasse -> FundoClasse (Many-to-One)
        // Configured in FundoClasseConfiguration via HasMany

        // Indexes
        builder.HasIndex(s => s.ClasseId)
            .HasDatabaseName("ix_subclasse_classe");

        builder.HasIndex(s => new { s.ClasseId, s.CodigoSubclasse })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_subclasse_codigo");

        // Query filter for soft delete
        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}
