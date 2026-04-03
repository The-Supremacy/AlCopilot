using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class OutboxSourceRegistrationTests
{
    [Fact]
    public void AddOutboxSource_AddsDescriptorOnce_ForDuplicateRegistration()
    {
        var services = new ServiceCollection();

        services.AddOutboxSource<TestDbContext>("drink-catalog", "drink_catalog", "domain_events");
        services.AddOutboxSource<TestDbContext>("drink-catalog", "drink_catalog", "domain_events");

        services
            .Where(descriptor => descriptor.ServiceType == typeof(OutboxSourceDescriptor))
            .ShouldHaveSingleItem()
            .ImplementationInstance.ShouldBe(
                new OutboxSourceDescriptor("drink-catalog", typeof(TestDbContext), "drink_catalog", "domain_events"));
    }

    [Fact]
    public void AddOutboxSource_Throws_WhenTypeIsNotDbContext()
    {
        var services = new ServiceCollection();

        Should.Throw<InvalidOperationException>(
            () => services.AddOutboxSource("invalid", typeof(string), "drink_catalog", "domain_events"));
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options);
}
