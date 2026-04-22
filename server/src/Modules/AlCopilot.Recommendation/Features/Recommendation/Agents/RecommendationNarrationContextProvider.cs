using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed record RecommendationRunContext(
    CustomerProfileDto Profile,
    IReadOnlyCollection<RecommendationGroupDto> RecommendationGroups,
    IReadOnlyDictionary<Guid, string> IngredientNames,
    IReadOnlyCollection<RecommendationRunContextGroup> Groups);

public sealed record RecommendationRunContextGroup(
    string Key,
    string Label,
    IReadOnlyCollection<RecommendationRunContextItem> Items);

public sealed record RecommendationRunContextItem(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    IReadOnlyCollection<string> OwnedIngredientNames,
    IReadOnlyCollection<string> MissingIngredientNames,
    IReadOnlyCollection<string> RecipeIngredientNames,
    string? Method,
    string? Garnish,
    int Score);

internal sealed class RecommendationRunContextProvider(IServiceScopeFactory scopeFactory) : MessageAIContextProvider
{
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customerMessage = context.RequestMessages
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
        if (string.IsNullOrWhiteSpace(customerMessage))
        {
            return [];
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var queryService = scope.ServiceProvider.GetRequiredService<IRecommendationRunContextQueryService>();
        var runContext = await queryService.GetRunContextAsync(customerMessage, cancellationToken);

        return
        [
            new ChatMessage(
                ChatRole.System,
                RecommendationRunContextMessageBuilder.Build(runContext)),
        ];
    }
}
