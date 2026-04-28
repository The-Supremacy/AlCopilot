using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.Recommendation.Data.Configurations;

internal sealed class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id)
            .ValueGeneratedNever();

        builder.Property(session => session.CustomerId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(session => session.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(session => session.AgentSessionStateJson)
            .HasColumnType("text");

        builder.Property(session => session.CreatedAtUtc)
            .IsRequired();

        builder.Property(session => session.UpdatedAtUtc)
            .IsRequired();

        builder.Ignore(session => session.DomainEvents);
    }
}
