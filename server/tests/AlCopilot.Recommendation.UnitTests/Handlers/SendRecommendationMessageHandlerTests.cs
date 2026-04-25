using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class SubmitRecommendationRequestHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToWorkflowForAuthenticatedCustomer()
    {
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var conversationService = Substitute.For<IRecommendationConversationService>();
        actorAccessor.GetCurrent().Returns(new CurrentActor("customer-1", "customer@example.com", true, ["user"]));
        conversationService.SendMessageAsync("customer-1", null, "Give me something citrusy", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RecommendationSessionDto(
                Guid.NewGuid(),
                "Citrusy ideas",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                new List<RecommendationTurnDto>())));

        var handler = new SubmitRecommendationRequestHandler(
            actorAccessor,
            conversationService);

        var result = await handler.Handle(
            new SubmitRecommendationRequestCommand(null, "Give me something citrusy"),
            CancellationToken.None);

        result.Title.ShouldBe("Citrusy ideas");
        await conversationService.Received(1).SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Throws_WhenActorIsAnonymous()
    {
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var conversationService = Substitute.For<IRecommendationConversationService>();
        actorAccessor.GetCurrent().Returns(CurrentActor.Anonymous);

        var handler = new SubmitRecommendationRequestHandler(
            actorAccessor,
            conversationService);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.Handle(
                new SubmitRecommendationRequestCommand(null, "Give me something citrusy"),
                CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_PassesThroughWorkflowValidation()
    {
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var conversationService = Substitute.For<IRecommendationConversationService>();
        actorAccessor.GetCurrent().Returns(new CurrentActor("customer-1", "customer@example.com", true, ["user"]));
        conversationService.SendMessageAsync("customer-1", null, "   ", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RecommendationSessionDto>(
                new AlCopilot.Shared.Errors.ValidationException("Recommendation message is required.")));

        var handler = new SubmitRecommendationRequestHandler(
            actorAccessor,
            conversationService);

        await Should.ThrowAsync<AlCopilot.Shared.Errors.ValidationException>(() =>
            handler.Handle(
                new SubmitRecommendationRequestCommand(null, "   "),
                CancellationToken.None).AsTask());
    }
}
