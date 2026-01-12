using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoVinculo entity.
/// </summary>
public class FundoVinculoConfiguration : IEntityTypeConfiguration<FundoVinculo>
{
    public void Configure(EntityTypeBuilder<FundoVinculo> builder)
    {
        builder.ToTable("fundo_vinculo", "cadastros");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.FundoId)
            .HasColumnName("fundo_id")
            .IsRequired();

        builder.Property(v => v.InstituicaoId)
            .HasColumnName("instituicao_id")
            .IsRequired();

        // Enum: TipoVinculoInstitucional (string storage)
        builder.Property(v => v.TipoVinculo)
            .HasColumnName("tipo_vinculo")
            .HasMaxLength(30)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(v => v.DataInicio)
            .HasColumnName("data_inicio")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(v => v.DataFim)
            .HasColumnName("data_fim")
            .HasColumnType("date");

        builder.Property(v => v.ContratoNumero)
            .HasColumnName("contrato_numero")
            .HasMaxLength(50);

        builder.Property(v => v.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(500);

        builder.Property(v => v.Principal)
            .HasColumnName("principal")
            .HasDefaultValue(false)
            .IsRequired();

        // Audit fields
        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(v => v.FundoId)
            .HasDatabaseName("ix_vinculo_fundo");

        builder.HasIndex(v => v.TipoVinculo)
            .HasDatabaseName("ix_vinculo_tipo");

        builder.HasIndex(v => new { v.FundoId, v.TipoVinculo })
            .HasFilter("data_fim IS NULL")
            .HasDatabaseName("ix_vinculo_vigente");

        // Relationships
        builder.HasOne(v => v.Fundo)
            .WithMany()
            .HasForeignKey(v => v.FundoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Instituicao)
            .WithMany(i => i.Vinculos)
            .HasForeignKey(v => v.InstituicaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
