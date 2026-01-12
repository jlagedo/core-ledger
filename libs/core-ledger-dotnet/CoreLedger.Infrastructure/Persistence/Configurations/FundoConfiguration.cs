using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Entity Framework Core configuration for Fundo entity.
/// </summary>
public class FundoConfiguration : IEntityTypeConfiguration<Fundo>
{
    public void Configure(EntityTypeBuilder<Fundo> builder)
    {
        builder.ToTable("fundo", "cadastros");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // Value Object: CNPJ
        builder.Property(f => f.Cnpj)
            .HasColumnName("cnpj")
            .HasMaxLength(14)
            .HasConversion(
                v => v.Valor,
                v => CNPJ.Criar(v))
            .IsRequired();

        builder.Property(f => f.RazaoSocial)
            .HasColumnName("razao_social")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.NomeFantasia)
            .HasColumnName("nome_fantasia")
            .HasMaxLength(100);

        builder.Property(f => f.NomeCurto)
            .HasColumnName("nome_curto")
            .HasMaxLength(30);

        builder.Property(f => f.DataConstituicao)
            .HasColumnName("data_constituicao")
            .HasColumnType("date");

        builder.Property(f => f.DataInicioAtividade)
            .HasColumnName("data_inicio_atividade")
            .HasColumnType("date");

        // Enum: TipoFundo (string storage)
        builder.Property(f => f.TipoFundo)
            .HasColumnName("tipo_fundo")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: ClassificacaoCVM (string storage)
        builder.Property(f => f.ClassificacaoCVM)
            .HasColumnName("classificacao_cvm")
            .HasMaxLength(30)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.ClassificacaoAnbima)
            .HasColumnName("classificacao_anbima")
            .HasMaxLength(100);

        // Value Object: CodigoANBIMA (nullable)
        builder.Property(f => f.CodigoAnbima)
            .HasColumnName("codigo_anbima")
            .HasMaxLength(6)
            .HasConversion(
                v => v == null ? null : v.Valor,
                v => v == null ? null : CodigoANBIMA.Criar(v));

        // Enum: SituacaoFundo (string storage)
        builder.Property(f => f.Situacao)
            .HasColumnName("situacao")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: PrazoFundo (string storage)
        builder.Property(f => f.Prazo)
            .HasColumnName("prazo")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: PublicoAlvo (string storage)
        builder.Property(f => f.PublicoAlvo)
            .HasColumnName("publico_alvo")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: TributacaoFundo (string storage)
        builder.Property(f => f.Tributacao)
            .HasColumnName("tributacao")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        // Enum: TipoCondominio (string storage)
        builder.Property(f => f.Condominio)
            .HasColumnName("condominio")
            .HasMaxLength(10)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.Exclusivo)
            .HasColumnName("exclusivo")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.Reservado)
            .HasColumnName("reservado")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.PermiteAlavancagem)
            .HasColumnName("permite_alavancagem")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.AceitaCripto)
            .HasColumnName("aceita_cripto")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.PercentualExterior)
            .HasColumnName("percentual_exterior")
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(f => f.WizardCompleto)
            .HasColumnName("wizard_completo")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.ProgressoCadastro)
            .HasColumnName("progresso_cadastro")
            .HasDefaultValue(0)
            .IsRequired();

        // Audit fields
        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(f => f.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(f => f.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(f => f.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(f => f.Cnpj)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_fundo_cnpj");

        builder.HasIndex(f => f.Situacao)
            .HasDatabaseName("ix_fundo_situacao");

        builder.HasIndex(f => f.TipoFundo)
            .HasDatabaseName("ix_fundo_tipo");

        // Query filter for soft delete
        builder.HasQueryFilter(f => f.DeletedAt == null);
    }
}
