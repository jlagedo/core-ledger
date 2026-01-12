using CoreLedger.Domain.Cadastros.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoPrazoExcecao entity.
/// </summary>
public class FundoPrazoExcecaoConfiguration : IEntityTypeConfiguration<FundoPrazoExcecao>
{
    public void Configure(EntityTypeBuilder<FundoPrazoExcecao> builder)
    {
        builder.ToTable("fundo_prazo_excecao", "cadastros");

        builder.HasKey(fpe => fpe.Id);

        builder.Property(fpe => fpe.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(fpe => fpe.PrazoId)
            .HasColumnName("prazo_id")
            .IsRequired();

        builder.Property(fpe => fpe.DataInicio)
            .HasColumnName("data_inicio")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(fpe => fpe.DataFim)
            .HasColumnName("data_fim")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(fpe => fpe.DiasCotizacao)
            .HasColumnName("dias_cotizacao")
            .IsRequired();

        builder.Property(fpe => fpe.DiasLiquidacao)
            .HasColumnName("dias_liquidacao")
            .IsRequired();

        builder.Property(fpe => fpe.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(fpe => fpe.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Relacionamento
        builder.HasOne(fpe => fpe.Prazo)
            .WithMany(fp => fp.Excecoes)
            .HasForeignKey(fpe => fpe.PrazoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice
        builder.HasIndex(fpe => fpe.PrazoId)
            .HasDatabaseName("ix_prazo_excecao_prazo");
    }
}
