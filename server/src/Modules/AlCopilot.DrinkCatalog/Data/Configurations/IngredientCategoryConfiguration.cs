using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class IngredientCategoryConfiguration : IEntityTypeConfiguration<IngredientCategory>
{
    public void Configure(EntityTypeBuilder<IngredientCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .HasConversion(v => v.Value, raw => CategoryName.Create(raw))
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(c => c.Name).IsUnique();
        builder.Ignore(c => c.DomainEvents);
    }
}
