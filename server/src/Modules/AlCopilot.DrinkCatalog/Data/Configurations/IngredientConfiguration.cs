using AlCopilot.DrinkCatalog.Features.Ingredient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name)
            .HasConversion(v => v.Value, raw => IngredientName.Create(raw))
            .IsRequired()
            .HasMaxLength(200);
        builder.HasIndex(i => i.Name).IsUnique();
        builder.Property(i => i.NotableBrands).HasColumnType("jsonb");
        builder.Ignore(i => i.DomainEvents);
    }
}
