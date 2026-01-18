using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for FundoTaxa entity.
/// </summary>
public class FundoTaxaConfiguration : IEntityTypeConfiguration<FundoTaxa>
{
    public void Configure(EntityTypeBuilder<FundoTaxa> builder)
    {
        builder.ToTable("fundo_taxa", "cadastros");

        builder.HasKey(t => t.Id);

        // PK: BIGINT SERIAL (alto volume)
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        // FK: Fundo (obrigatório)
        builder.Property(t => t.FundoId)
            .HasColumnName("fundo_id")
            .IsRequired();

        // FK: Classe (opcional - taxa por classe)
        builder.Property(t => t.ClasseId)
            .HasColumnName("classe_id");

        // Enum: TipoTaxa (string storage)
        builder.Property(t => t.TipoTaxa)
            .HasColumnName("tipo_taxa")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Percentual: decimal(8,4) para % a.a.
        builder.Property(t => t.Percentual)
            .HasColumnName("percentual")
            .HasColumnType("decimal(8,4)")
            .IsRequired();

        // Enum: BaseCalculoTaxa (string storage)
        builder.Property(t => t.BaseCalculo)
            .HasColumnName("base_calculo")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: PeriodicidadeProvisao (string storage)
        builder.Property(t => t.PeriodicidadeProvisao)
            .HasColumnName("periodicidade_provisao")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: PeriodicidadePagamento (string storage)
        builder.Property(t => t.PeriodicidadePagamento)
            .HasColumnName("periodicidade_pagamento")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Dia do mês para pagamento (1-28)
        builder.Property(t => t.DiaPagamento)
            .HasColumnName("dia_pagamento");

        // Valor mínimo mensal: decimal(18,2)
        builder.Property(t => t.ValorMinimo)
            .HasColumnName("valor_minimo")
            .HasColumnType("decimal(18,2)");

        // Valor máximo (cap): decimal(18,2)
        builder.Property(t => t.ValorMaximo)
            .HasColumnName("valor_maximo")
            .HasColumnType("decimal(18,2)");

        // Data de início da vigência
        builder.Property(t => t.DataInicioVigencia)
            .HasColumnName("data_inicio_vigencia")
            .HasColumnType("date")
            .IsRequired();

        // Data de fim da vigência (null = vigente)
        builder.Property(t => t.DataFimVigencia)
            .HasColumnName("data_fim_vigencia")
            .HasColumnType("date");

        // Indica se a taxa está ativa
        builder.Property(t => t.Ativa)
            .HasColumnName("ativa")
            .HasDefaultValue(true)
            .IsRequired();

        // Linha d'água global (para taxa de performance)
        builder.Property(t => t.LinhaDaguaGlobal)
            .HasColumnName("linha_dagua_global");

        // Audit fields
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationship: FundoTaxa -> Fundo (Many-to-One)
        builder.HasOne(t => t.Fundo)
            .WithMany(f => f.Taxas)
            .HasForeignKey(t => t.FundoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: FundoTaxa -> FundoClasse (Many-to-One, opcional)
        builder.HasOne(t => t.Classe)
            .WithMany(c => c.Taxas)
            .HasForeignKey(t => t.ClasseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: FundoTaxa -> FundoTaxaPerformance (One-to-One)
        builder.HasOne(t => t.ParametrosPerformance)
            .WithOne(p => p.FundoTaxa)
            .HasForeignKey<FundoTaxaPerformance>(p => p.FundoTaxaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.FundoId)
            .HasDatabaseName("ix_taxa_fundo");

        builder.HasIndex(t => t.ClasseId)
            .HasDatabaseName("ix_taxa_classe");

        builder.HasIndex(t => t.TipoTaxa)
            .HasDatabaseName("ix_taxa_tipo");

        // Unique constraint: apenas uma taxa ativa por tipo/fundo/classe
        builder.HasIndex(t => new { t.FundoId, t.ClasseId, t.TipoTaxa, t.Ativa })
            .IsUnique()
            .HasFilter("ativa = true")
            .HasDatabaseName("ix_taxa_unica_ativa");

        // Query filter: match parent Fundo's soft delete filter
        builder.HasQueryFilter(t => t.Fundo.DeletedAt == null);
    }
}
