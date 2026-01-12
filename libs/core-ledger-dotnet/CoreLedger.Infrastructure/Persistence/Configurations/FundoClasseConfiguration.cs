using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoClasse entity.
/// </summary>
public class FundoClasseConfiguration : IEntityTypeConfiguration<FundoClasse>
{
    public void Configure(EntityTypeBuilder<FundoClasse> builder)
    {
        builder.ToTable("fundo_classe", "cadastros");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.FundoId)
            .HasColumnName("fundo_id")
            .IsRequired();

        builder.Property(c => c.CnpjClasse)
            .HasColumnName("cnpj_classe")
            .HasMaxLength(14);

        builder.Property(c => c.CodigoClasse)
            .HasColumnName("codigo_classe")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.NomeClasse)
            .HasColumnName("nome_classe")
            .HasMaxLength(100)
            .IsRequired();

        // Enum: TipoClasseFIDC (string storage, nullable)
        builder.Property(c => c.TipoClasseFidc)
            .HasColumnName("tipo_classe_fidc")
            .HasMaxLength(20)
            .HasConversion<string?>();

        builder.Property(c => c.OrdemSubordinacao)
            .HasColumnName("ordem_subordinacao");

        builder.Property(c => c.RentabilidadeAlvo)
            .HasColumnName("rentabilidade_alvo")
            .HasColumnType("decimal(8,4)");

        builder.Property(c => c.ResponsabilidadeLimitada)
            .HasColumnName("responsabilidade_limitada")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.SegregacaoPatrimonial)
            .HasColumnName("segregacao_patrimonial")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.ValorMinimoAplicacao)
            .HasColumnName("valor_minimo_aplicacao")
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Ativa)
            .HasColumnName("ativa")
            .HasDefaultValue(true)
            .IsRequired();

        // Permite resgate antecipado
        builder.Property(c => c.PermiteResgateAntecipado)
            .HasColumnName("permite_resgate_antecipado")
            .HasDefaultValue(true)
            .IsRequired();

        // Data de encerramento da classe
        builder.Property(c => c.DataEncerramento)
            .HasColumnName("data_encerramento")
            .HasColumnType("date");

        // Motivo do encerramento
        builder.Property(c => c.MotivoEncerramento)
            .HasColumnName("motivo_encerramento")
            .HasMaxLength(500);

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationship: FundoClasse -> Fundo (Many-to-One)
        builder.HasOne(c => c.Fundo)
            .WithMany(f => f.Classes)
            .HasForeignKey(c => c.FundoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: FundoClasse -> FundoSubclasse (One-to-Many)
        builder.HasMany(c => c.Subclasses)
            .WithOne(s => s.Classe)
            .HasForeignKey(s => s.ClasseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.FundoId)
            .HasDatabaseName("ix_classe_fundo");

        builder.HasIndex(c => new { c.FundoId, c.CodigoClasse })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_classe_codigo");

        // Query filter for soft delete
        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}
