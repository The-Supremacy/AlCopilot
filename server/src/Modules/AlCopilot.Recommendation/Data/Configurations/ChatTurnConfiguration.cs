using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.Recommendation.Data.Configurations;

internal sealed class ChatTurnConfiguration : IEntityTypeConfiguration<ChatTurn>
{
    public void Configure(EntityTypeBuilder<ChatTurn> builder)
    {
        builder.HasKey(turn => turn.Id);
        builder.Property(turn => turn.Id)
            .ValueGeneratedNever();

        builder.Property(turn => turn.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(turn => turn.Content)
            .IsRequired();

        builder.Property(turn => turn.RecommendationGroupsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(turn => turn.ToolInvocationsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(turn => turn.CreatedAtUtc)
            .IsRequired();
    }
}
