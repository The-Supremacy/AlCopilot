using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.Recommendation.Data;

public sealed class RecommendationDbContext(DbContextOptions<RecommendationDbContext> options)
    : DbContext(options), IRecommendationUnitOfWork
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<AgentRun> AgentRuns => Set<AgentRun>();
    public DbSet<AgentMessage> AgentMessages => Set<AgentMessage>();
    public DbSet<AgentMessageDiagnostic> AgentMessageDiagnostics => Set<AgentMessageDiagnostic>();
    public DbSet<RecommendationTurnGroup> RecommendationTurnGroups => Set<RecommendationTurnGroup>();
    public DbSet<RecommendationTurnItem> RecommendationTurnItems => Set<RecommendationTurnItem>();
    public DbSet<RecommendationTurnItemMissingIngredient> RecommendationTurnItemMissingIngredients => Set<RecommendationTurnItemMissingIngredient>();
    public DbSet<RecommendationTurnItemMatchedSignal> RecommendationTurnItemMatchedSignals => Set<RecommendationTurnItemMatchedSignal>();
    public DbSet<RecommendationTurnItemRecipeEntry> RecommendationTurnItemRecipeEntries => Set<RecommendationTurnItemRecipeEntry>();
    public DbSet<DomainEventRecord> DomainEventRecords => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("recommendation");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecommendationDbContext).Assembly);
    }
}
