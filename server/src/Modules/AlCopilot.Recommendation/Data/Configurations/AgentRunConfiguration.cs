using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.Recommendation.Data.Configurations;

internal sealed class AgentRunConfiguration : IEntityTypeConfiguration<AgentRun>
{
    public void Configure(EntityTypeBuilder<AgentRun> builder)
    {
        builder.HasKey(run => run.Id);
        builder.Property(run => run.Id)
            .ValueGeneratedNever();

        builder.Property(run => run.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(run => run.Provider)
            .HasMaxLength(100);

        builder.Property(run => run.Model)
            .HasMaxLength(200);

        builder.Property(run => run.FinishReason)
            .HasMaxLength(100);

        builder.Property(run => run.ErrorSummary)
            .HasMaxLength(1000);

        builder.Property(run => run.StartedAtUtc)
            .IsRequired();

        builder.HasIndex(run => run.ChatSessionId);

        builder.HasOne<ChatSession>()
            .WithMany()
            .HasForeignKey(run => run.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
