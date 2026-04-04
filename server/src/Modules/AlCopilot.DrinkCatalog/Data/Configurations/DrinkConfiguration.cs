using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class DrinkConfiguration : IEntityTypeConfiguration<Drink>
{
    public void Configure(EntityTypeBuilder<Drink> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name)
            .HasConversion(v => v.Value, raw => DrinkName.Create(raw))
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(2000);
        builder.Property(d => d.ImageUrl)
            .HasConversion(v => v.Value, raw => ImageUrl.Create(raw))
            .IsRequired(false)
            .HasMaxLength(1000);
        builder.HasIndex(d => d.Name).IsUnique();
        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.HasMany(d => d.Tags)
            .WithMany()
            .UsingEntity("DrinkTag",
                l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagId"),
                r => r.HasOne(typeof(Drink)).WithMany().HasForeignKey("DrinkId"));

        builder.HasMany(d => d.RecipeEntries)
            .WithOne()
            .HasForeignKey(e => e.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Tags).AutoInclude();
        builder.Navigation(d => d.RecipeEntries).AutoInclude();

        builder.Ignore(d => d.DomainEvents);
    }
}
