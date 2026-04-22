using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using System.Diagnostics;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationRunContextFactory(
    IRecommendationRunInputsQueryService runInputsQueryService,
    IRecommendationRequestIntentResolver requestIntentResolver,
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder) : IRecommendationRunContextFactory
{
    public async Task<RecommendationRunContext> CreateAsync(
        string customerMessage,
        CancellationToken cancellationToken = default)
    {
        using var activity = RecommendationTelemetry.ActivitySource.StartActivity(
            "recommendation.run_context.build",
            ActivityKind.Internal);
        activity?.SetTag("recommendation.customer_message.length", customerMessage.Length);

        var inputs = await runInputsQueryService.GetRunInputsAsync(cancellationToken);
        var intent = requestIntentResolver.Resolve(customerMessage, inputs);
        var groups = candidateBuilder.Build(customerMessage, intent, inputs.Profile, inputs.Drinks);
        var ownedIngredientIds = inputs.Profile.OwnedIngredientIds.ToHashSet();
        var drinkLookup = inputs.Drinks.ToDictionary(drink => drink.Id);
        var ingredientNames = inputs.Drinks
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

        activity?.SetTag("recommendation.intent.kind", intent.Kind.ToString());
        activity?.SetTag("recommendation.groups.count", runContextGroups.Count);
        activity?.SetTag("recommendation.items.count", runContextGroups.Sum(group => group.Items.Count));
        executionTraceRecorder.Record(
            new RecommendationExecutionTraceStep(
                "run_context.build",
                "ok",
                $"Built {runContextGroups.Count} recommendation group(s) for {intent.Kind}.",
                DateTimeOffset.UtcNow,
                new Dictionary<string, string?>
                {
                    ["intentKind"] = intent.Kind.ToString(),
                    ["requestedDrinkName"] = intent.RequestedDrinkName,
                    ["requestedIngredientName"] = intent.RequestedIngredientName,
                    ["matchedPreferenceSignals"] = string.Join(", ", intent.PreferenceSignals),
                    ["groupCount"] = runContextGroups.Count.ToString(),
                    ["itemCount"] = runContextGroups.Sum(group => group.Items.Count).ToString(),
                },
                []));

        return new RecommendationRunContext(intent, inputs.Profile, groups, ingredientNames, runContextGroups);
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
                item.MatchedSignals,
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
            item.MatchedSignals,
            item.Score);
    }
}
