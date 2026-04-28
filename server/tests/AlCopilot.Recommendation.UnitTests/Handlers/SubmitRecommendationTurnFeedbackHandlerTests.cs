using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class SubmitRecommendationTurnFeedbackHandlerTests
{
    [Fact]
    public async Task Handle_RecordsFeedback_AndSavesWithoutReloadingSessionDto()
    {
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var chatSessionRepository = Substitute.For<IChatSessionRepository>();
        var agentMessageRepository = Substitute.For<IAgentMessageRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var session = ChatSession.Create("customer-1", "First request");
        var message = CreateAgentMessage(session.Id, "assistant", "Try the Gimlet.");

        actorAccessor.GetCurrent().Returns(new CurrentActor("customer-1", "customer@example.com", true, ["user"]));
        chatSessionRepository.GetByCustomerSessionIdAsync(
                "customer-1",
                session.Id,
                Arg.Any<CancellationToken>())
            .Returns(session);
        agentMessageRepository.GetBySessionMessageIdAsync(
                session.Id,
                message.Id,
                Arg.Any<CancellationToken>())
            .Returns(message);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new SubmitRecommendationTurnFeedbackHandler(
            actorAccessor,
            chatSessionRepository,
            agentMessageRepository,
            unitOfWork);

        var result = await handler.Handle(
            new SubmitRecommendationTurnFeedbackCommand(session.Id, message.Id, "positive", "Great fit."),
            CancellationToken.None);

        result.ShouldBe(Mediator.Unit.Value);
        message.FeedbackRating.ShouldBe("positive");
        message.FeedbackComment.ShouldBe("Great fit.");
        message.FeedbackCreatedAtUtc.ShouldNotBeNull();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static AgentMessage CreateAgentMessage(Guid sessionId, string role, string text) =>
        AgentMessage.Create(
            sessionId,
            Guid.NewGuid(),
            1,
            Guid.NewGuid().ToString("N"),
            role,
            "text",
            "maf",
            text,
            "{}");
}
