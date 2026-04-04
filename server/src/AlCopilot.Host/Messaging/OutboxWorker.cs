using System.Text.Json;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Bus;

namespace AlCopilot.Host.Messaging;

public sealed class OutboxWorker(
    IServiceProvider serviceProvider,
    IEnumerable<OutboxSourceDescriptor> sources,
    DomainEventTypeRegistry eventTypeRegistry,
    IBus bus,
    IOptions<OutboxWorkerOptions> options,
    ILogger<OutboxWorker> logger) : BackgroundService
{
    private readonly IReadOnlyList<OutboxSourceDescriptor> _sources = sources.ToList();
    private readonly OutboxWorkerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_sources.Count == 0)
        {
            logger.LogInformation("No outbox sources registered. Durable publishing worker is idle.");
            return;
        }

        using var timer = new PeriodicTimer(_options.PollInterval);

        do
        {
            foreach (var source in _sources)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                await ProcessSourceAsync(source, stoppingToken);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessSourceAsync(OutboxSourceDescriptor source, CancellationToken cancellationToken)
    {
        List<long> pendingIds;

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = ResolveDbContext(scope.ServiceProvider, source);

            pendingIds = await dbContext.Set<DomainEventRecord>()
                .Where(record => record.DispatchedAtUtc == null)
                .OrderBy(record => record.Id)
                .Take(_options.BatchSize)
                .Select(record => record.Id)
                .ToListAsync(cancellationToken);
        }

        foreach (var recordId in pendingIds)
        {
            try
            {
                await ProcessRecordAsync(source, recordId, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(
                    exception,
                    "Failed to publish outbox record {RecordId} from source {SourceName}. The record will remain undispatched.",
                    recordId,
                    source.Name);
            }
        }
    }

    private async Task ProcessRecordAsync(
        OutboxSourceDescriptor source,
        long recordId,
        CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = ResolveDbContext(scope.ServiceProvider, source);

        var record = await dbContext.Set<DomainEventRecord>()
            .SingleOrDefaultAsync(r => r.Id == recordId, cancellationToken);

        if (record is null || record.DispatchedAtUtc is not null)
        {
            return;
        }

        var eventType = eventTypeRegistry.GetType(record.EventType);
        var @event = JsonSerializer.Deserialize(record.Payload, eventType)
            ?? throw new InvalidOperationException(
                $"Outbox record {record.Id} payload could not be deserialized as '{eventType.FullName}'.");

        await bus.Publish(@event);

        record.DispatchedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DbContext ResolveDbContext(IServiceProvider serviceProvider, OutboxSourceDescriptor source)
    {
        var dbContext = serviceProvider.GetRequiredService(source.DbContextType) as DbContext;

        return dbContext ?? throw new InvalidOperationException(
            $"Outbox source '{source.Name}' resolved '{source.DbContextType.FullName}', which is not a DbContext.");
    }
}
