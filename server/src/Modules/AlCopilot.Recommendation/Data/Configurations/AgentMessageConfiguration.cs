using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.Recommendation.Data.Configurations;

internal sealed class AgentMessageConfiguration : IEntityTypeConfiguration<AgentMessage>
{
    public void Configure(EntityTypeBuilder<AgentMessage> builder)
    {
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.NativeMessageId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(message => message.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(message => message.Kind)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(message => message.Source)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(message => message.RawMessageJson)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(message => message.FeedbackRating)
            .HasMaxLength(20);

        builder.Property(message => message.FeedbackComment)
            .HasMaxLength(1000);

        builder.Property(message => message.FeedbackCreatedAtUtc);

        builder.Property(message => message.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(message => new { message.ChatSessionId, message.Sequence })
            .IsUnique();

        builder.HasIndex(message => new { message.ChatSessionId, message.NativeMessageId })
            .IsUnique();

        builder.HasIndex(message => message.AgentRunId);

        builder.HasOne<ChatSession>()
            .WithMany()
            .HasForeignKey(message => message.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AgentRun>()
            .WithMany()
            .HasForeignKey(message => message.AgentRunId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
