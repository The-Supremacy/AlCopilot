using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class DomainEventRecordConfiguration : IEntityTypeConfiguration<DomainEventRecord>
{
    public void Configure(EntityTypeBuilder<DomainEventRecord> builder)
    {
        builder.ToTable("domain_events", "drink_catalog");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseIdentityByDefaultColumn();
        builder.Property(r => r.AggregateType).IsRequired().HasMaxLength(200);
        builder.Property(r => r.EventType).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Payload).IsRequired().HasColumnType("jsonb");
        builder.HasIndex(r => new { r.AggregateId, r.Id });
        builder.HasIndex(r => r.OccurredAtUtc);
        builder.HasIndex(r => new { r.DispatchedAtUtc, r.Id })
            .HasFilter("\"DispatchedAtUtc\" IS NULL");
    }
}
