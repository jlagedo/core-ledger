using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

/// <summary>
///     Configuração EF Core para a entidade CoreJob.
/// </summary>
public class CoreJobConfiguration : IEntityTypeConfiguration<CoreJob>
{
    public void Configure(EntityTypeBuilder<CoreJob> builder)
    {
        builder.ToTable("core_jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(j => j.ReferenceId)
            .HasColumnName("reference_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(j => j.JobDescription)
            .HasColumnName("job_description")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(j => j.CreationDate)
            .HasColumnName("creation_date")
            .IsRequired();

        builder.Property(j => j.RunningDate)
            .HasColumnName("running_date");

        builder.Property(j => j.FinishedDate)
            .HasColumnName("finished_date");

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at");

        // Índice não único para desempenho de consulta
        builder.HasIndex(j => j.ReferenceId);

        // Índice para filtrar por status
        builder.HasIndex(j => j.Status);
    }
}