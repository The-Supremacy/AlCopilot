using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlCopilot.Recommendation.Data.Configurations;

internal sealed class AgentMessageDiagnosticConfiguration : IEntityTypeConfiguration<AgentMessageDiagnostic>
{
    public void Configure(EntityTypeBuilder<AgentMessageDiagnostic> builder)
    {
        builder.HasKey(diagnostic => diagnostic.Id);
        builder.Property(diagnostic => diagnostic.Id)
            .ValueGeneratedNever();

        builder.Property(diagnostic => diagnostic.Kind)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(diagnostic => diagnostic.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(diagnostic => diagnostic.RawPayloadJson)
            .HasColumnType("jsonb");

        builder.Property(diagnostic => diagnostic.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(diagnostic => diagnostic.ChatSessionId);
        builder.HasIndex(diagnostic => diagnostic.AgentRunId);
        builder.HasIndex(diagnostic => diagnostic.AgentMessageId);

        builder.HasOne<ChatSession>()
            .WithMany()
            .HasForeignKey(diagnostic => diagnostic.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AgentRun>()
            .WithMany()
            .HasForeignKey(diagnostic => diagnostic.AgentRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AgentMessage>()
            .WithMany()
            .HasForeignKey(diagnostic => diagnostic.AgentMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
