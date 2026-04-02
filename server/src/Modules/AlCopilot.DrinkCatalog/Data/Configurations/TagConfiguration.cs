using AlCopilot.DrinkCatalog.Features.Tag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name)
            .HasConversion(v => v.Value, raw => TagName.Create(raw))
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(t => t.Name).IsUnique();
        builder.Ignore(t => t.DomainEvents);
    }
}
