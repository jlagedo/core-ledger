using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoPrazo entity.
/// </summary>
public class FundoPrazoConfiguration : IEntityTypeConfiguration<FundoPrazo>
{
    public void Configure(EntityTypeBuilder<FundoPrazo> builder)
    {
        builder.ToTable("fundo_prazo", "cadastros");

        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(fp => fp.FundoId)
            .HasColumnName("fundo_id")
            .IsRequired();

        builder.Property(fp => fp.ClasseId)
            .HasColumnName("classe_id");

        builder.Property(fp => fp.TipoPrazo)
            .HasColumnName("tipo_prazo")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<TipoPrazoOperacional>(v))
            .IsRequired();

        builder.Property(fp => fp.DiasCotizacao)
            .HasColumnName("dias_cotizacao")
            .IsRequired();

        builder.Property(fp => fp.DiasLiquidacao)
            .HasColumnName("dias_liquidacao")
            .IsRequired();

        builder.Property(fp => fp.DiasCarencia)
            .HasColumnName("dias_carencia");

        builder.Property(fp => fp.HorarioLimite)
            .HasColumnName("horario_limite")
            .HasColumnType("time")
            .IsRequired();

        builder.Property(fp => fp.DiasUteis)
            .HasColumnName("dias_uteis")
            .IsRequired();

        builder.Property(fp => fp.CalendarioId)
            .HasColumnName("calendario_id");

        builder.Property(fp => fp.PermiteParcial)
            .HasColumnName("permite_parcial")
            .IsRequired();

        builder.Property(fp => fp.PercentualMinimo)
            .HasColumnName("percentual_minimo")
            .HasColumnType("decimal(5,2)");

        builder.Property(fp => fp.ValorMinimo)
            .HasColumnName("valor_minimo")
            .HasColumnType("decimal(18,2)");

        // Tipo de calendário para contagem D+X
        builder.Property(fp => fp.TipoCalendario)
            .HasColumnName("tipo_calendario")
            .HasMaxLength(20)
            .HasDefaultValue("NACIONAL")
            .IsRequired();

        // Permite resgate programado/agendado
        builder.Property(fp => fp.PermiteResgateProgramado)
            .HasColumnName("permite_resgate_programado")
            .HasDefaultValue(false)
            .IsRequired();

        // Prazo máximo para programar resgate
        builder.Property(fp => fp.PrazoMaximoProgramacao)
            .HasColumnName("prazo_maximo_programacao");

        builder.Property(fp => fp.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(fp => fp.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(fp => fp.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        // Relacionamentos
        builder.HasOne(fp => fp.Fundo)
            .WithMany()
            .HasForeignKey(fp => fp.FundoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fp => fp.Classe)
            .WithMany()
            .HasForeignKey(fp => fp.ClasseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(fp => fp.Excecoes)
            .WithOne(e => e.Prazo)
            .HasForeignKey(e => e.PrazoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(fp => fp.FundoId)
            .HasDatabaseName("ix_prazo_fundo");

        builder.HasIndex(fp => fp.ClasseId)
            .HasDatabaseName("ix_prazo_classe");

        builder.HasIndex(fp => new { fp.FundoId, fp.ClasseId, fp.TipoPrazo })
            .HasDatabaseName("ix_prazo_tipo")
            .HasFilter("ativo = true")
            .IsUnique();
    }
}
