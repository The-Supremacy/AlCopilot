using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.StrategyKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.SourceFingerprint)
            .HasMaxLength(500);

        ConfigureJsonProperty(
            builder.Property(b => b.Provenance),
            ImportProvenance.Empty,
            JsonValueComparerFactory.Create<ImportProvenance>());

        ConfigureJsonProperty(
            builder.Property(b => b.ImportContent),
            new NormalizedCatalogImport([], [], []),
            JsonValueComparerFactory.Create<NormalizedCatalogImport>());

        ConfigureJsonProperty(
            builder.Property(b => b.Diagnostics),
            new List<ImportDiagnostic>(),
            JsonValueComparerFactory.Create<List<ImportDiagnostic>>());

        ConfigureJsonProperty(
            builder.Property(b => b.ReviewConflicts),
            new List<ImportReviewConflict>(),
            JsonValueComparerFactory.Create<List<ImportReviewConflict>>());

        ConfigureJsonProperty(
            builder.Property(b => b.ReviewRows),
            new List<ImportReviewRow>(),
            JsonValueComparerFactory.Create<List<ImportReviewRow>>());

        ConfigureJsonProperty(
            builder.Property(b => b.DecisionAuditTrail),
            new List<ImportDecisionAuditEntry>(),
            JsonValueComparerFactory.Create<List<ImportDecisionAuditEntry>>());

        ConfigureJsonProperty(
            builder.Property(b => b.ReviewSummary),
            default(ImportReviewSummary?),
            JsonValueComparerFactory.Create<ImportReviewSummary?>());

        ConfigureJsonProperty(
            builder.Property(b => b.ApplySummary),
            default(ImportApplySummary?),
            JsonValueComparerFactory.Create<ImportApplySummary?>());

        builder.HasIndex(b => new { b.StrategyKey, b.SourceFingerprint, b.Status });
        builder.HasIndex(b => b.CreatedAtUtc);

        builder.Ignore(b => b.DomainEvents);
    }

    private static void ConfigureJsonProperty<T>(
        PropertyBuilder<T> propertyBuilder,
        T defaultValue,
        Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<T> valueComparer)
    {
        propertyBuilder
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonValueComparerFactory.Serialize(value),
                value => value == null ? defaultValue : JsonValueComparerFactory.Deserialize<T>(value));

        propertyBuilder.Metadata.SetValueComparer(valueComparer);
    }
}
