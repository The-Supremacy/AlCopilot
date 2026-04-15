using AlCopilot.CustomerProfile.Features.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.CustomerProfile.Data.Configurations;

internal sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<Features.Profile.CustomerProfile>
{
    public void Configure(EntityTypeBuilder<Features.Profile.CustomerProfile> builder)
    {
        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.CustomerId)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(profile => profile.CustomerId)
            .IsUnique();

        builder.Property(profile => profile.FavoriteIngredientIds)
            .IsRequired()
            .HasColumnType("uuid[]");

        builder.Property(profile => profile.DislikedIngredientIds)
            .IsRequired()
            .HasColumnType("uuid[]");

        builder.Property(profile => profile.ProhibitedIngredientIds)
            .IsRequired()
            .HasColumnType("uuid[]");

        builder.Property(profile => profile.OwnedIngredientIds)
            .IsRequired()
            .HasColumnType("uuid[]");

        builder.Property(profile => profile.CreatedAtUtc)
            .IsRequired();

        builder.Property(profile => profile.UpdatedAtUtc)
            .IsRequired();

        builder.Ignore(profile => profile.DomainEvents);
    }
}
