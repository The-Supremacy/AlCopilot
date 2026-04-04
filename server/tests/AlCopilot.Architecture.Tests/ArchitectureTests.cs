using NetArchTest.Rules;
using Shouldly;

namespace AlCopilot.Architecture.Tests;

public sealed class ModuleBoundaryTests
{
    private static readonly System.Reflection.Assembly DrinkCatalogAssembly =
        typeof(AlCopilot.DrinkCatalog.DrinkCatalogModule).Assembly;

    [Fact]
    public void DrinkCatalog_ShouldNotReference_OtherModuleImplementations()
    {
        // Currently only one module exists. This test validates the pattern
        // and will catch violations when additional modules are added.
        var result = Types.InAssembly(DrinkCatalogAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "AlCopilot.Recommendation",
                "AlCopilot.UserProfile")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"DrinkCatalog references other module implementations: {FormatFailing(result)}");
    }

    private static string FormatFailing(TestResult result) =>
        string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []);
}

public sealed class ContractsPurityTests
{
    private static readonly System.Reflection.Assembly ContractsAssembly =
        typeof(AlCopilot.DrinkCatalog.Contracts.DTOs.DrinkDto).Assembly;

    [Fact]
    public void Contracts_ShouldContainOnly_InterfacesDtosRecordsAndMessages()
    {
        var types = Types.InAssembly(ContractsAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotNested()
            .GetTypes();

        foreach (var archType in types)
        {
            var type = archType.ReflectionType;
            var isRecord = type.GetMethod("<Clone>$") is not null;
            var isSealed = type.IsSealed;
            var isAbstract = type.IsAbstract;

            // All concrete classes in Contracts must be sealed records (DTOs/messages)
            (isRecord && isSealed || isAbstract).ShouldBeTrue(
                $"'{type.FullName}' in Contracts is not a sealed record or abstract class");
        }
    }

    [Fact]
    public void Contracts_ShouldNotContain_EfCoreTypes()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Contracts references EF Core: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }
}

public sealed class SealedClassTests
{
    private static readonly System.Reflection.Assembly DrinkCatalogAssembly =
        typeof(AlCopilot.DrinkCatalog.DrinkCatalogModule).Assembly;

    [Fact]
    public void AllClasses_InDrinkCatalog_ShouldBeSealed()
    {
        var result = Types.InAssembly(DrinkCatalogAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .AreNotStatic()
            .And()
            .DoNotResideInNamespaceContaining("Migrations")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Non-sealed classes found: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }
}

public sealed class HandlerConventionTests
{
    private static readonly System.Reflection.Assembly DrinkCatalogAssembly =
        typeof(AlCopilot.DrinkCatalog.DrinkCatalogModule).Assembly;

    [Fact]
    public void Handlers_ShouldNotReference_DbContextDirectly()
    {
        var result = Types.InAssembly(DrinkCatalogAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOnAny("AlCopilot.DrinkCatalog.Data.DrinkCatalogDbContext")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Handlers reference DbContext directly: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }
}
