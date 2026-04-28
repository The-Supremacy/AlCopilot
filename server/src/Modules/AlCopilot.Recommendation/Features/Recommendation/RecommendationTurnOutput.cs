using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class RecommendationTurnGroup
{
    public Guid Id { get; private set; }
    public Guid AgentRunId { get; private set; }
    public int Sequence { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public List<RecommendationTurnItem> Items { get; private set; } = [];

    private RecommendationTurnGroup()
    {
    }

    public static List<RecommendationTurnGroup> CreateMany(
        Guid agentRunId,
        IReadOnlyCollection<RecommendationGroupDto> groups)
    {
        var result = new List<RecommendationTurnGroup>();
        var sequence = 1;
        foreach (var group in groups)
        {
            result.Add(Create(agentRunId, sequence, group));
            sequence++;
        }

        return result;
    }

    private static RecommendationTurnGroup Create(Guid agentRunId, int sequence, RecommendationGroupDto dto)
    {
        var group = new RecommendationTurnGroup
        {
            Id = Guid.NewGuid(),
            AgentRunId = agentRunId,
            Sequence = sequence,
            Key = dto.Key,
            Label = dto.Label,
        };

        var itemSequence = 1;
        foreach (var item in dto.Items)
        {
            group.Items.Add(RecommendationTurnItem.Create(group.Id, itemSequence, item));
            itemSequence++;
        }

        return group;
    }

    public RecommendationGroupDto ToDto() =>
        new(
            Key,
            Label,
            Items
                .OrderBy(item => item.Sequence)
                .Select(item => item.ToDto())
                .ToList());
}

public sealed class RecommendationTurnItem
{
    public Guid Id { get; private set; }
    public Guid RecommendationTurnGroupId { get; private set; }
    public Guid DrinkId { get; private set; }
    public int Sequence { get; private set; }
    public string DrinkName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Score { get; private set; }
    public List<RecommendationTurnItemMissingIngredient> MissingIngredients { get; private set; } = [];
    public List<RecommendationTurnItemMatchedSignal> MatchedSignals { get; private set; } = [];
    public List<RecommendationTurnItemRecipeEntry> RecipeEntries { get; private set; } = [];

    private RecommendationTurnItem()
    {
    }

    public static RecommendationTurnItem Create(Guid groupId, int sequence, RecommendationItemDto dto)
    {
        var item = new RecommendationTurnItem
        {
            Id = Guid.NewGuid(),
            RecommendationTurnGroupId = groupId,
            DrinkId = dto.DrinkId,
            Sequence = sequence,
            DrinkName = dto.DrinkName,
            Description = dto.Description,
            Score = dto.Score,
        };

        var missingSequence = 1;
        foreach (var missingIngredient in dto.MissingIngredientNames)
        {
            item.MissingIngredients.Add(RecommendationTurnItemMissingIngredient.Create(item.Id, missingSequence, missingIngredient));
            missingSequence++;
        }

        var signalSequence = 1;
        foreach (var signal in dto.MatchedSignals)
        {
            item.MatchedSignals.Add(RecommendationTurnItemMatchedSignal.Create(item.Id, signalSequence, signal));
            signalSequence++;
        }

        var recipeSequence = 1;
        foreach (var entry in dto.RecipeEntries ?? [])
        {
            item.RecipeEntries.Add(RecommendationTurnItemRecipeEntry.Create(item.Id, recipeSequence, entry));
            recipeSequence++;
        }

        return item;
    }

    public RecommendationItemDto ToDto() =>
        new(
            DrinkId,
            DrinkName,
            Description,
            MissingIngredients.OrderBy(item => item.Sequence).Select(item => item.IngredientName).ToList(),
            MatchedSignals.OrderBy(item => item.Sequence).Select(item => item.Signal).ToList(),
            Score,
            RecipeEntries.Count == 0
                ? null
                : RecipeEntries
                    .OrderBy(item => item.Sequence)
                    .Select(item => new RecommendationRecipeEntryDto(item.IngredientName, item.Quantity, item.IsOwned))
                    .ToList());
}

public sealed class RecommendationTurnItemMissingIngredient
{
    public Guid Id { get; private set; }
    public Guid RecommendationTurnItemId { get; private set; }
    public int Sequence { get; private set; }
    public string IngredientName { get; private set; } = string.Empty;

    private RecommendationTurnItemMissingIngredient()
    {
    }

    public static RecommendationTurnItemMissingIngredient Create(Guid itemId, int sequence, string ingredientName) =>
        new()
        {
            Id = Guid.NewGuid(),
            RecommendationTurnItemId = itemId,
            Sequence = sequence,
            IngredientName = ingredientName,
        };
}

public sealed class RecommendationTurnItemMatchedSignal
{
    public Guid Id { get; private set; }
    public Guid RecommendationTurnItemId { get; private set; }
    public int Sequence { get; private set; }
    public string Signal { get; private set; } = string.Empty;

    private RecommendationTurnItemMatchedSignal()
    {
    }

    public static RecommendationTurnItemMatchedSignal Create(Guid itemId, int sequence, string signal) =>
        new()
        {
            Id = Guid.NewGuid(),
            RecommendationTurnItemId = itemId,
            Sequence = sequence,
            Signal = signal,
        };
}

public sealed class RecommendationTurnItemRecipeEntry
{
    public Guid Id { get; private set; }
    public Guid RecommendationTurnItemId { get; private set; }
    public int Sequence { get; private set; }
    public string IngredientName { get; private set; } = string.Empty;
    public string Quantity { get; private set; } = string.Empty;
    public bool IsOwned { get; private set; }

    private RecommendationTurnItemRecipeEntry()
    {
    }

    public static RecommendationTurnItemRecipeEntry Create(Guid itemId, int sequence, RecommendationRecipeEntryDto dto) =>
        new()
        {
            Id = Guid.NewGuid(),
            RecommendationTurnItemId = itemId,
            Sequence = sequence,
            IngredientName = dto.IngredientName,
            Quantity = dto.Quantity,
            IsOwned = dto.IsOwned,
        };
}
