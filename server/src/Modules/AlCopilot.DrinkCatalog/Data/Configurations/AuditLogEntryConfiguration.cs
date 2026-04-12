using AlCopilot.DrinkCatalog.Features.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entry => entry.SubjectType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entry => entry.SubjectKey)
            .HasMaxLength(200);

        builder.Property(entry => entry.Actor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entry => entry.Summary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entry => entry.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(entry => entry.OccurredAtUtc);
        builder.HasIndex(entry => new { entry.SubjectType, entry.SubjectKey });
    }
}
