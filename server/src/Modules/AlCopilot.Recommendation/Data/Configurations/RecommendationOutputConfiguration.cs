using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.Recommendation.Data.Configurations;

internal sealed class RecommendationTurnGroupConfiguration : IEntityTypeConfiguration<RecommendationTurnGroup>
{
    public void Configure(EntityTypeBuilder<RecommendationTurnGroup> builder)
    {
        builder.HasKey(group => group.Id);
        builder.Property(group => group.Id)
            .ValueGeneratedNever();

        builder.Property(group => group.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(group => group.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(group => new { group.AgentRunId, group.Sequence })
            .IsUnique();

        builder.HasOne<AgentRun>()
            .WithMany()
            .HasForeignKey(group => group.AgentRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(group => group.Items)
            .WithOne()
            .HasForeignKey(item => item.RecommendationTurnGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RecommendationTurnItemConfiguration : IEntityTypeConfiguration<RecommendationTurnItem>
{
    public void Configure(EntityTypeBuilder<RecommendationTurnItem> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.DrinkName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(item => new { item.RecommendationTurnGroupId, item.Sequence })
            .IsUnique();

        builder.HasMany(item => item.MissingIngredients)
            .WithOne()
            .HasForeignKey(missingIngredient => missingIngredient.RecommendationTurnItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(item => item.MatchedSignals)
            .WithOne()
            .HasForeignKey(signal => signal.RecommendationTurnItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(item => item.RecipeEntries)
            .WithOne()
            .HasForeignKey(entry => entry.RecommendationTurnItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RecommendationTurnItemMissingIngredientConfiguration
    : IEntityTypeConfiguration<RecommendationTurnItemMissingIngredient>
{
    public void Configure(EntityTypeBuilder<RecommendationTurnItemMissingIngredient> builder)
    {
        builder.HasKey(ingredient => ingredient.Id);
        builder.Property(ingredient => ingredient.Id)
            .ValueGeneratedNever();

        builder.Property(ingredient => ingredient.IngredientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(ingredient => new { ingredient.RecommendationTurnItemId, ingredient.Sequence })
            .IsUnique();
    }
}

internal sealed class RecommendationTurnItemMatchedSignalConfiguration
    : IEntityTypeConfiguration<RecommendationTurnItemMatchedSignal>
{
    public void Configure(EntityTypeBuilder<RecommendationTurnItemMatchedSignal> builder)
    {
        builder.HasKey(signal => signal.Id);
        builder.Property(signal => signal.Id)
            .ValueGeneratedNever();

        builder.Property(signal => signal.Signal)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(signal => new { signal.RecommendationTurnItemId, signal.Sequence })
            .IsUnique();
    }
}

internal sealed class RecommendationTurnItemRecipeEntryConfiguration
    : IEntityTypeConfiguration<RecommendationTurnItemRecipeEntry>
{
    public void Configure(EntityTypeBuilder<RecommendationTurnItemRecipeEntry> builder)
    {
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.IngredientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entry => entry.Quantity)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(entry => new { entry.RecommendationTurnItemId, entry.Sequence })
            .IsUnique();
    }
}
