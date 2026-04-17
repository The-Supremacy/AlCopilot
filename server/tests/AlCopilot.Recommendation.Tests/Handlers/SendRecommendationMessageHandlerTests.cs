using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using AlCopilot.Shared.Models;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class SubmitRecommendationRequestHandlerTests
{
    [Fact]
    public async Task Handle_CreatesSessionAndPersistsStructuredAssistantTurn()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var mediator = Substitute.For<IMediator>();
        var candidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();
        var narrator = Substitute.For<IRecommendationNarrator>();
        actorAccessor.GetCurrent().Returns(new CurrentActor("customer-1", "customer@example.com", true, ["user"]));

        mediator.Send(Arg.Any<GetCustomerProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(new CustomerProfileDto([], [], [], []));
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
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
        narrator.GenerateAsync(Arg.Any<RecommendationNarrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationNarrationResult("Try the Gimlet.", []));

        var handler = new SubmitRecommendationRequestHandler(
            repository,
            unitOfWork,
            actorAccessor,
            mediator,
            candidateBuilder,
            narrator);

        var result = await handler.Handle(
            new SubmitRecommendationRequestCommand(null, "Give me something citrusy"),
            CancellationToken.None);

        result.Turns.Count.ShouldBe(2);
        result.Turns.Last().Role.ShouldBe("assistant");
        result.Turns.Last().RecommendationGroups.Count.ShouldBeGreaterThan(0);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Throws_WhenActorIsAnonymous()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var mediator = Substitute.For<IMediator>();
        var candidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();
        var narrator = Substitute.For<IRecommendationNarrator>();
        actorAccessor.GetCurrent().Returns(CurrentActor.Anonymous);

        var handler = new SubmitRecommendationRequestHandler(
            repository,
            unitOfWork,
            actorAccessor,
            mediator,
            candidateBuilder,
            narrator);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.Handle(
                new SubmitRecommendationRequestCommand(null, "Give me something citrusy"),
                CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_Throws_WhenMessageIsBlank()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var actorAccessor = Substitute.For<ICurrentActorAccessor>();
        var mediator = Substitute.For<IMediator>();
        var candidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();
        var narrator = Substitute.For<IRecommendationNarrator>();
        actorAccessor.GetCurrent().Returns(new CurrentActor("customer-1", "customer@example.com", true, ["user"]));

        var handler = new SubmitRecommendationRequestHandler(
            repository,
            unitOfWork,
            actorAccessor,
            mediator,
            candidateBuilder,
            narrator);

        await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(
                new SubmitRecommendationRequestCommand(null, "   "),
                CancellationToken.None).AsTask());
    }
}
