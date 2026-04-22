using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Mediator;
using System.Diagnostics;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationRunContextQueryService(
    IMediator mediator,
    IRecommendationCandidateBuilder candidateBuilder) : IRecommendationRunContextQueryService
{
    public async Task<RecommendationRunContext> GetRunContextAsync(
        string customerMessage,
        CancellationToken cancellationToken = default)
    {
        using var activity = RecommendationTelemetry.ActivitySource.StartActivity(
            "recommendation.run_context.build",
            ActivityKind.Internal);
        activity?.SetTag("recommendation.customer_message.length", customerMessage.Length);

        var profile = await mediator.Send(new GetCustomerProfileQuery(), cancellationToken);
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), cancellationToken);
        var groups = candidateBuilder.Build(customerMessage, profile, drinks);
        var ownedIngredientIds = profile.OwnedIngredientIds.ToHashSet();
        var drinkLookup = drinks.ToDictionary(drink => drink.Id);
        var ingredientNames = drinks
            .SelectMany(drink => drink.RecipeEntries)
            .Select(entry => entry.Ingredient)
            .GroupBy(ingredient => ingredient.Id)
            .ToDictionary(
                group => group.Key,
                group => group.Select(ingredient => ingredient.Name)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .First());
        var runContextGroups = groups
            .Select(group => new RecommendationRunContextGroup(
                group.Key,
                group.Label,
                group.Items
                    .Select(item => BuildRunContextItem(item, drinkLookup, ownedIngredientIds))
                    .ToList()))
            .ToList();

        activity?.SetTag("recommendation.groups.count", runContextGroups.Count);
        activity?.SetTag("recommendation.items.count", runContextGroups.Sum(group => group.Items.Count));

        return new RecommendationRunContext(profile, groups, ingredientNames, runContextGroups);
    }

    private static RecommendationRunContextItem BuildRunContextItem(
        RecommendationItemDto item,
        IReadOnlyDictionary<Guid, DrinkDetailDto> drinkLookup,
        HashSet<Guid> ownedIngredientIds)
    {
        if (!drinkLookup.TryGetValue(item.DrinkId, out var drink))
        {
            return new RecommendationRunContextItem(
                item.DrinkId,
                item.DrinkName,
                item.Description,
                [],
                item.MissingIngredientNames,
                [],
                null,
                null,
                item.Score);
        }

        var ownedIngredientNames = drink.RecipeEntries
            .Where(entry => ownedIngredientIds.Contains(entry.Ingredient.Id))
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var recipeIngredientNames = drink.RecipeEntries
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new RecommendationRunContextItem(
            drink.Id,
            drink.Name,
            string.IsNullOrWhiteSpace(item.Description) ? drink.Description : item.Description,
            ownedIngredientNames,
            item.MissingIngredientNames,
            recipeIngredientNames,
            drink.Method,
            drink.Garnish,
            item.Score);
    }
}
