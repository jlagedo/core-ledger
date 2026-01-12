using CoreLedger.Domain.Cadastros.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoParametrosCota entity.
/// </summary>
public class FundoParametrosCotaConfiguration : IEntityTypeConfiguration<FundoParametrosCota>
{
    public void Configure(EntityTypeBuilder<FundoParametrosCota> builder)
    {
        builder.ToTable("fundo_parametros_cota", "cadastros");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.FundoId)
            .HasColumnName("fundo_id")
            .IsRequired();

        builder.Property(p => p.CasasDecimaisCota)
            .HasColumnName("casas_decimais_cota")
            .HasDefaultValue(8)
            .IsRequired();

        builder.Property(p => p.CasasDecimaisQuantidade)
            .HasColumnName("casas_decimais_quantidade")
            .HasDefaultValue(6)
            .IsRequired();

        builder.Property(p => p.CasasDecimaisPl)
            .HasColumnName("casas_decimais_pl")
            .HasDefaultValue(2)
            .IsRequired();

        // Enum: TipoCota (string storage)
        builder.Property(p => p.TipoCota)
            .HasColumnName("tipo_cota")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.HorarioCorte)
            .HasColumnName("horario_corte")
            .HasColumnType("time")
            .IsRequired();

        builder.Property(p => p.CotaInicial)
            .HasColumnName("cota_inicial")
            .HasColumnType("decimal(20,8)")
            .IsRequired();

        builder.Property(p => p.DataCotaInicial)
            .HasColumnName("data_cota_inicial")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.FusoHorario)
            .HasColumnName("fuso_horario")
            .HasMaxLength(50)
            .HasDefaultValue("America/Sao_Paulo")
            .IsRequired();

        builder.Property(p => p.PermiteCotaEstimada)
            .HasColumnName("permite_cota_estimada")
            .HasDefaultValue(false)
            .IsRequired();

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Relacionamento 1:1 com Fundo
        builder.HasOne(p => p.Fundo)
            .WithOne(f => f.ParametrosCota)
            .HasForeignKey<FundoParametrosCota>(p => p.FundoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único para garantir 1:1
        builder.HasIndex(p => p.FundoId)
            .IsUnique()
            .HasDatabaseName("ix_parametros_cota_fundo");
    }
}
