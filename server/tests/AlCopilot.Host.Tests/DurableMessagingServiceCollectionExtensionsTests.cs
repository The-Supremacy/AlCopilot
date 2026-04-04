using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Host.Messaging;
using AlCopilot.Shared.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class DurableMessagingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDurableMessaging_DisablesFeature_WhenMessagingConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        services.AddDurableMessaging(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.IsDurableMessagingEnabled().ShouldBeFalse();
        services.ShouldNotContain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationType == typeof(OutboxWorker));
    }

    [Fact]
    public void AddDurableMessaging_RegistersEnabledState_HostedWorker_AndBoundOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:messaging"] = "Endpoint=sb://alcopilot.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                [$"{OutboxWorkerOptions.SectionName}:BatchSize"] = "42",
                [$"{OutboxWorkerOptions.SectionName}:PollInterval"] = "00:00:07"
            })
            .Build();

        services.AddDurableMessaging(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.IsDurableMessagingEnabled().ShouldBeTrue();
        services.ShouldContain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationType == typeof(OutboxWorker));

        var options = serviceProvider.GetRequiredService<IOptions<OutboxWorkerOptions>>().Value;
        options.BatchSize.ShouldBe(42);
        options.PollInterval.ShouldBe(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void AddDurableMessaging_RegistersResolvableBus_WhenMessagingIsEnabled()
    {
        var services = new ServiceCollection();
        services.AddSingleton(DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:messaging"] = "Endpoint=sb://alcopilot.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test"
            })
            .Build();

        services.AddDurableMessaging(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IBus>().ShouldNotBeNull();
    }
}
