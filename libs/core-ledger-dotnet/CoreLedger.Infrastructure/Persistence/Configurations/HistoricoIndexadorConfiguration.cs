using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for HistoricoIndexador entity.
/// </summary>
public class HistoricoIndexadorConfiguration : IEntityTypeConfiguration<HistoricoIndexador>
{
    public void Configure(EntityTypeBuilder<HistoricoIndexador> builder)
    {
        builder.ToTable("historicos_indexadores");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(h => h.IndexadorId)
            .HasColumnName("indexador_id")
            .IsRequired();

        builder.Property(h => h.DataReferencia)
            .HasColumnName("data_referencia")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(h => h.Valor)
            .HasColumnName("valor")
            .HasColumnType("decimal(20,12)")
            .IsRequired();

        builder.Property(h => h.FatorDiario)
            .HasColumnName("fator_diario")
            .HasColumnType("decimal(20,12)");

        builder.Property(h => h.VariacaoPercentual)
            .HasColumnName("variacao_percentual")
            .HasColumnType("decimal(12,8)");

        builder.Property(h => h.Fonte)
            .HasColumnName("fonte")
            .HasMaxLength(50);

        builder.Property(h => h.ImportacaoId)
            .HasColumnName("importacao_id");

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Ignore UpdatedAt - historical data is immutable
        builder.Ignore(h => h.UpdatedAt);

        // Foreign key relationship
        builder.HasOne(h => h.Indexador)
            .WithMany()
            .HasForeignKey(h => h.IndexadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on (indexador_id, data_referencia)
        builder.HasIndex(h => new { h.IndexadorId, h.DataReferencia })
            .IsUnique()
            .HasDatabaseName("ix_historicos_indexadores_indexador_data");

        builder.HasIndex(h => h.IndexadorId)
            .HasDatabaseName("ix_historicos_indexadores_indexador_id");

        builder.HasIndex(h => h.DataReferencia)
            .HasDatabaseName("ix_historicos_indexadores_data_referencia");
    }
}
