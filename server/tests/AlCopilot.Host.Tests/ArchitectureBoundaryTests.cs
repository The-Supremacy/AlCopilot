using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Shared.Domain;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void ContractsAssembly_ShouldNotDependOn_DrinkCatalogImplementationNamespaces()
    {
        var result = Types.InAssembly(typeof(DrinkCreatedEvent).Assembly)
            .ShouldNot()
            .HaveDependencyOn("AlCopilot.DrinkCatalog.Features")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DrinkCatalogImplementation_ShouldNotDefineConcreteDomainEvents()
    {
        var concreteDomainEvents = typeof(Drink).Assembly
            .GetTypes()
            .Where(type => typeof(IDomainEvent).IsAssignableFrom(type))
            .Where(type => !type.IsInterface && !type.IsAbstract)
            .ToArray();

        concreteDomainEvents.ShouldBeEmpty();
    }
}
