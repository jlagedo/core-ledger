using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Configuração Entity Framework Core para a entidade Calendario.
/// </summary>
public class CalendarioConfiguration : IEntityTypeConfiguration<Calendario>
{
    public void Configure(EntityTypeBuilder<Calendario> builder)
    {
        builder.ToTable("calendarios");

        // Chave primária
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        // Data (DateOnly)
        builder.Property(c => c.Data)
            .HasColumnName("data")
            .IsRequired();

        // DiaUtil (computado a partir de TipoDia)
        builder.Property(c => c.DiaUtil)
            .HasColumnName("dia_util")
            .IsRequired();

        // TipoDia (enum armazenado como int)
        builder.Property(c => c.TipoDia)
            .HasColumnName("tipo_dia")
            .HasConversion<int>()
            .IsRequired();

        // Praca (enum armazenado como int)
        builder.Property(c => c.Praca)
            .HasColumnName("praca")
            .HasConversion<int>()
            .IsRequired();

        // Descricao (opcional, máx 100 caracteres)
        builder.Property(c => c.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(100);

        // CreatedByUserId
        builder.Property(c => c.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired()
            .HasMaxLength(200);

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // CAL-001: Índice composto único em (data, praca)
        builder.HasIndex(c => new { c.Data, c.Praca })
            .IsUnique()
            .HasDatabaseName("ix_calendarios_data_praca");

        // Índice adicional para consulta por data
        builder.HasIndex(c => c.Data)
            .HasDatabaseName("ix_calendarios_data");

        // Índice adicional para consulta por praca
        builder.HasIndex(c => c.Praca)
            .HasDatabaseName("ix_calendarios_praca");
    }
}
