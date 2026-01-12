using System.Text.Json;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for FundoParametrosFIDC entity.
/// </summary>
public class FundoParametrosFIDCConfiguration : IEntityTypeConfiguration<FundoParametrosFIDC>
{
    public void Configure(EntityTypeBuilder<FundoParametrosFIDC> builder)
    {
        builder.ToTable("fundo_parametros_fidc", "cadastros");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.FundoId)
            .HasColumnName("fundo_id")
            .IsRequired();

        // Enum: TipoFIDC (string storage)
        builder.Property(p => p.TipoFidc)
            .HasColumnName("tipo_fidc")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // List<TipoRecebiveis> armazenado como JSON
        builder.Property(p => p.TiposRecebiveis)
            .HasColumnName("tipos_recebiveis")
            .HasMaxLength(500)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<TipoRecebiveis>>(v, (JsonSerializerOptions?)null)
                     ?? new List<TipoRecebiveis>())
            .IsRequired();

        builder.Property(p => p.PrazoMedioCarteira)
            .HasColumnName("prazo_medio_carteira");

        builder.Property(p => p.IndiceSubordinacaoAlvo)
            .HasColumnName("indice_subordinacao_alvo")
            .HasColumnType("decimal(5,4)");

        builder.Property(p => p.IndiceSubordinacaoMinimo)
            .HasColumnName("indice_subordinacao_minimo")
            .HasColumnType("decimal(5,4)");

        builder.Property(p => p.ProvisaoDevedoresDuvidosos)
            .HasColumnName("provisao_devedores_duvidosos")
            .HasColumnType("decimal(5,4)");

        builder.Property(p => p.LimiteConcentracaoCedente)
            .HasColumnName("limite_concentracao_cedente")
            .HasColumnType("decimal(5,4)");

        builder.Property(p => p.LimiteConcentracaoSacado)
            .HasColumnName("limite_concentracao_sacado")
            .HasColumnType("decimal(5,4)");

        builder.Property(p => p.PossuiCoobrigacao)
            .HasColumnName("possui_coobrigacao")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.PercentualCoobrigacao)
            .HasColumnName("percentual_coobrigacao")
            .HasColumnType("decimal(5,4)");

        // Enum: Registradora (string storage, nullable)
        builder.Property(p => p.RegistradoraRecebiveis)
            .HasColumnName("registradora_recebiveis")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.IntegracaoRegistradora)
            .HasColumnName("integracao_registradora")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.CodigoRegistradora)
            .HasColumnName("codigo_registradora")
            .HasMaxLength(50);

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Relacionamento 1:1 com Fundo
        builder.HasOne(p => p.Fundo)
            .WithOne(f => f.ParametrosFIDC)
            .HasForeignKey<FundoParametrosFIDC>(p => p.FundoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único para garantir 1:1
        builder.HasIndex(p => p.FundoId)
            .IsUnique()
            .HasDatabaseName("ix_parametros_fidc_fundo");
    }
}
