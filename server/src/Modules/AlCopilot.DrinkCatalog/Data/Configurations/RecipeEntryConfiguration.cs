using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class RecipeEntryConfiguration : IEntityTypeConfiguration<RecipeEntry>
{
    public void Configure(EntityTypeBuilder<RecipeEntry> builder)
    {
        builder.HasKey(e => new { e.DrinkId, e.IngredientId });
        builder.Property(e => e.Quantity)
            .HasConversion(v => v.Value, raw => Quantity.Create(raw))
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(e => e.RecommendedBrand).HasMaxLength(200);
        builder.HasOne<Ingredient>()
            .WithMany()
            .HasForeignKey(e => e.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
