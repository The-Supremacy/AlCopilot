using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Workflows;
using AlCopilot.Shared.Errors;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationWorkflowTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesSessionAndPersistsStructuredAssistantTurn()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        var candidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();
        var narrator = Substitute.For<IRecommendationNarrator>();

        mediator.Send(Arg.Any<AlCopilot.CustomerProfile.Contracts.Queries.GetCustomerProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(new CustomerProfileDto([], [], [], []));
        mediator.Send(Arg.Any<AlCopilot.DrinkCatalog.Contracts.Queries.GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new DrinkDetailDto(
                    Guid.NewGuid(),
                    "Gimlet",
                    null,
                    "Bright and citrusy",
                    null,
                    null,
                    null,
                    [],
                    [new RecipeEntryDto(new IngredientDto(Guid.NewGuid(), "Gin", []), "2 oz", null)])
            ]);
        candidateBuilder.Build(
                Arg.Any<string>(),
                Arg.Any<CustomerProfileDto>(),
                Arg.Any<IReadOnlyCollection<DrinkDetailDto>>())
            .Returns(
            [
                new RecommendationGroupDto(
                    "make-now",
                    "Make Now",
                    [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", "Bright and citrusy", [], ["citrusy"], 100)])
            ]);
        var serializedSession = """{"stateBag":{"messages":[]}}""";
        narrator.NarrateAsync(Arg.Any<RecommendationNarrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationNarrationResult("Try the Gimlet.", [], serializedSession));

        var workflow = new RecommendationWorkflow(repository, unitOfWork, mediator, candidateBuilder, narrator);

        var result = await workflow.ExecuteAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        result.Turns.Count.ShouldBe(2);
        result.Turns.First().Role.ShouldBe("user");
        result.Turns.Last().Role.ShouldBe("assistant");
        result.Turns.Last().RecommendationGroups.Count.ShouldBeGreaterThan(0);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        repository.Received(1).Add(Arg.Is<ChatSession>(session =>
            session.AgentSessionStateJson == serializedSession));
    }

    [Fact]
    public async Task ExecuteAsync_ReusesExistingSessionWhenItBelongsToCustomer()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        var candidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();
        var narrator = Substitute.For<IRecommendationNarrator>();
        var existingSession = ChatSession.Create("customer-1", "First request");

        repository.GetByCustomerSessionIdAsync("customer-1", existingSession.Id, Arg.Any<CancellationToken>())
            .Returns(existingSession);
        mediator.Send(Arg.Any<AlCopilot.CustomerProfile.Contracts.Queries.GetCustomerProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(new CustomerProfileDto([], [], [], []));
        mediator.Send(Arg.Any<AlCopilot.DrinkCatalog.Contracts.Queries.GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<DrinkDetailDto>());
        candidateBuilder.Build(Arg.Any<string>(), Arg.Any<CustomerProfileDto>(), Arg.Any<IReadOnlyCollection<DrinkDetailDto>>())
            .Returns(
            [
                new RecommendationGroupDto("make-now", "Make Now", [])
            ]);
        var serializedSession = """{"stateBag":{"messages":[{"role":"user"}]}}""";
        narrator.NarrateAsync(Arg.Any<RecommendationNarrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationNarrationResult("No great matches right now.", [], serializedSession));

        var workflow = new RecommendationWorkflow(repository, unitOfWork, mediator, candidateBuilder, narrator);

        var result = await workflow.ExecuteAsync(
            "customer-1",
            existingSession.Id,
            "Something else",
            CancellationToken.None);

        result.SessionId.ShouldBe(existingSession.Id);
        repository.DidNotReceive().Add(Arg.Any<ChatSession>());
        result.Turns.Last().Role.ShouldBe("assistant");
        existingSession.AgentSessionStateJson.ShouldBe(serializedSession);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenMessageIsBlank()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        var candidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();
        var narrator = Substitute.For<IRecommendationNarrator>();
        var workflow = new RecommendationWorkflow(
            repository,
            unitOfWork,
            mediator,
            candidateBuilder,
            narrator);

        await Should.ThrowAsync<ValidationException>(() =>
            workflow.ExecuteAsync("customer-1", null, "   ", CancellationToken.None));
    }
}
