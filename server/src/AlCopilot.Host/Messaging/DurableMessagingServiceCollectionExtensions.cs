using AlCopilot.Shared.Domain;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Serialization.Custom;
using Rebus.Serialization.Json;
using Rebus.Topic;
using Rebus.ServiceProvider;

namespace AlCopilot.Host.Messaging;

public static class DurableMessagingServiceCollectionExtensions
{
    private const string InputQueueName = "alcopilot-host";

    public static IServiceCollection AddDurableMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("messaging");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IDurableMessagingState>(new DurableMessagingState(false));
            return services;
        }

        services.Configure<OutboxWorkerOptions>(configuration.GetSection(OutboxWorkerOptions.SectionName));
        services.AddSingleton<IDurableMessagingState>(new DurableMessagingState(true));
        services.AddHostedService<OutboxWorker>();

        services.AddRebus(
            (configure, provider) =>
            {
                var registry = provider.GetRequiredService<DomainEventTypeRegistry>();

                return configure
                    .Transport(t => t.UseAzureServiceBus(connectionString, InputQueueName)
                        .DoNotCreateQueues()
                        .DoNotCheckQueueConfiguration()
                        .DoNotConfigureTopic())
                    .Serialization(s =>
                    {
                        s.UseSystemTextJson();

                        var typeNameBuilder = s.UseCustomMessageTypeNames();
                        foreach (var pair in registry.GetTypeNames())
                        {
                            typeNameBuilder.AddWithCustomName(pair.Key, pair.Value);
                        }
                    })
                    .Options(o =>
                    {
                        o.SetNumberOfWorkers(0);
                        o.Register(c => new LogicalMessageTypeNameConvention(registry));
                        o.Decorate<ITopicNameConvention>(c => new LogicalTopicNameConvention(registry));
                    });
            });

        return services;
    }

    public static bool IsDurableMessagingEnabled(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IDurableMessagingState>().IsEnabled;
}

public interface IDurableMessagingState
{
    bool IsEnabled { get; }
}

public sealed record DurableMessagingState(bool IsEnabled) : IDurableMessagingState;
