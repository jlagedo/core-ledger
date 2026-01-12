using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for Indexador entity.
/// </summary>
public class IndexadorConfiguration : IEntityTypeConfiguration<Indexador>
{
    public void Configure(EntityTypeBuilder<Indexador> builder)
    {
        builder.ToTable("indexadores");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Tipo)
            .HasColumnName("tipo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.Fonte)
            .HasColumnName("fonte")
            .HasMaxLength(100);

        builder.Property(i => i.Periodicidade)
            .HasColumnName("periodicidade")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.FatorAcumulado)
            .HasColumnName("fator_acumulado")
            .HasColumnType("decimal(20,12)");

        builder.Property(i => i.DataBase)
            .HasColumnName("data_base")
            .HasColumnType("date");

        builder.Property(i => i.UrlFonte)
            .HasColumnName("url_fonte")
            .HasMaxLength(500);

        builder.Property(i => i.ImportacaoAutomatica)
            .HasColumnName("importacao_automatica")
            .IsRequired();

        builder.Property(i => i.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(i => i.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_indexadores_codigo");

        builder.HasIndex(i => i.Tipo)
            .HasDatabaseName("ix_indexadores_tipo");

        builder.HasIndex(i => i.Ativo)
            .HasDatabaseName("ix_indexadores_ativo");
    }
}
