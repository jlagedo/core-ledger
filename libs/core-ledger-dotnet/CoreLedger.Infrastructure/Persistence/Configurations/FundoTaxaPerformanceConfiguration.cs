using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for FundoTaxaPerformance entity.
/// </summary>
public class FundoTaxaPerformanceConfiguration : IEntityTypeConfiguration<FundoTaxaPerformance>
{
    public void Configure(EntityTypeBuilder<FundoTaxaPerformance> builder)
    {
        builder.ToTable("fundo_taxa_performance", "cadastros");

        builder.HasKey(p => p.Id);

        // PK: BIGINT SERIAL
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        // FK: FundoTaxa (obrigatório, 1:1)
        builder.Property(p => p.FundoTaxaId)
            .HasColumnName("fundo_taxa_id")
            .IsRequired();

        // FK: Indexador (benchmark, obrigatório)
        builder.Property(p => p.IndexadorId)
            .HasColumnName("indexador_id")
            .IsRequired();

        // Percentual do benchmark: decimal(8,4)
        builder.Property(p => p.PercentualBenchmark)
            .HasColumnName("percentual_benchmark")
            .HasColumnType("decimal(8,4)")
            .IsRequired();

        // Enum: MetodoCalculoPerformance (string storage)
        builder.Property(p => p.MetodoCalculo)
            .HasColumnName("metodo_calculo")
            .HasMaxLength(30)
            .HasConversion<string>()
            .IsRequired();

        // Indica se utiliza linha d'água
        builder.Property(p => p.LinhaDagua)
            .HasColumnName("linha_dagua")
            .HasDefaultValue(true)
            .IsRequired();

        // Enum: PeriodicidadeCristalizacao (string storage)
        builder.Property(p => p.PeriodicidadeCristalizacao)
            .HasColumnName("periodicidade_cristalizacao")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Mês de cristalização (1-12)
        builder.Property(p => p.MesCristalizacao)
            .HasColumnName("mes_cristalizacao");

        // Audit field
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationship: FundoTaxaPerformance -> Indexador (Many-to-One)
        builder.HasOne(p => p.Indexador)
            .WithMany()
            .HasForeignKey(p => p.IndexadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.FundoTaxaId)
            .IsUnique()
            .HasDatabaseName("ix_taxa_performance_taxa");

        builder.HasIndex(p => p.IndexadorId)
            .HasDatabaseName("ix_taxa_performance_indexador");
    }
}
