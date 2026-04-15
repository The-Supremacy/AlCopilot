using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.Recommendation.Data;

public sealed class RecommendationDbContext(DbContextOptions<RecommendationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatTurn> ChatTurns => Set<ChatTurn>();
    public DbSet<DomainEventRecord> DomainEventRecords => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("recommendation");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecommendationDbContext).Assembly);
    }
}
