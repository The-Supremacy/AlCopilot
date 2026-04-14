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

    [Fact]
    public void QueryHandlers_ShouldNotReference_AggregateRepositories()
    {
        var readHandlerTypes = new[]
        {
            typeof(AlCopilot.DrinkCatalog.Features.Drink.GetDrinksHandler),
            typeof(AlCopilot.DrinkCatalog.Features.Drink.GetDrinkByIdHandler),
            typeof(AlCopilot.DrinkCatalog.Features.Tag.GetTagsHandler),
            typeof(AlCopilot.DrinkCatalog.Features.Ingredient.GetIngredientsHandler),
            typeof(AlCopilot.DrinkCatalog.Features.Audit.GetAuditLogEntriesHandler),
        };

        foreach (var handlerType in readHandlerTypes)
        {
            var constructorParameterTypes = handlerType.GetConstructors()
                .Single()
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();

            constructorParameterTypes.ShouldNotContain(
                typeof(AlCopilot.DrinkCatalog.Features.Drink.IDrinkRepository),
                $"{handlerType.Name} should depend on a query service, not IDrinkRepository.");
            constructorParameterTypes.ShouldNotContain(
                typeof(AlCopilot.DrinkCatalog.Features.Tag.ITagRepository),
                $"{handlerType.Name} should depend on a query service, not ITagRepository.");
            constructorParameterTypes.ShouldNotContain(
                typeof(AlCopilot.DrinkCatalog.Features.Ingredient.IIngredientRepository),
                $"{handlerType.Name} should depend on a query service, not IIngredientRepository.");
            constructorParameterTypes.ShouldNotContain(
                typeof(AlCopilot.DrinkCatalog.Features.Audit.IAuditLogEntryRepository),
                $"{handlerType.Name} should depend on a query service, not IAuditLogEntryRepository.");
        }
    }
}

public sealed class RepositoryConventionTests
{
    [Fact]
    public void AggregateRepositories_ShouldNotReturn_ContractDtos()
    {
        var repositoryTypes = new[]
        {
            typeof(AlCopilot.DrinkCatalog.Features.Drink.IDrinkRepository),
            typeof(AlCopilot.DrinkCatalog.Features.Tag.ITagRepository),
            typeof(AlCopilot.DrinkCatalog.Features.Ingredient.IIngredientRepository),
        };

        foreach (var repositoryType in repositoryTypes)
        {
            var dtoReturningMethods = repositoryType.GetMethods()
                .Where(method => ReturnsContractDto(method.ReturnType))
                .Select(method => method.Name)
                .ToArray();

            dtoReturningMethods.ShouldBeEmpty(
                $"{repositoryType.Name} should stay aggregate-focused and not return contract DTOs.");
        }
    }

    private static bool ReturnsContractDto(Type type)
    {
        if (type.Namespace == "AlCopilot.DrinkCatalog.Contracts.DTOs")
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        return type.GetGenericArguments().Any(ReturnsContractDto);
    }
}

public sealed class ServiceConventionTests
{
    private static readonly System.Reflection.Assembly DrinkCatalogAssembly =
        typeof(AlCopilot.DrinkCatalog.DrinkCatalogModule).Assembly;

    [Fact]
    public void ValidationServices_ShouldNotReference_DbContextDirectly()
    {
        var result = Types.InAssembly(DrinkCatalogAssembly)
            .That()
            .HaveNameEndingWith("Validator")
            .ShouldNot()
            .HaveDependencyOnAny("AlCopilot.DrinkCatalog.Data.DrinkCatalogDbContext")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Validation services reference DbContext directly: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }
}
