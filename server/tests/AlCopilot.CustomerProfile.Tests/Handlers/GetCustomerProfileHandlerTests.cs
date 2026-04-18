using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.CustomerProfile.Features.Profile;
using AlCopilot.CustomerProfile.Features.Profile.Abstractions;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.CustomerProfile.Tests.Handlers;

public sealed class GetCustomerProfileHandlerTests
{
    [Fact]
    public async Task Handle_LoadsProfileForAuthenticatedCustomer()
    {
        var queryService = Substitute.For<ICustomerProfileQueryService>();
        var currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
        currentActorAccessor.GetCurrent().Returns(new CurrentActor("customer-42", "customer@example.com", true, ["user"]));
        queryService.GetByCustomerIdAsync("customer-42", Arg.Any<CancellationToken>())
            .Returns(new CustomerProfileDto([], [], [], [Guid.Parse("00000000-0000-0000-0000-000000000010")]));

        var handler = new GetCustomerProfileHandler(queryService, currentActorAccessor);

        var result = await handler.Handle(new GetCustomerProfileQuery(), CancellationToken.None);

        result.OwnedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000010")]);
    }

    [Fact]
    public async Task Handle_Throws_WhenActorIsAnonymous()
    {
        var queryService = Substitute.For<ICustomerProfileQueryService>();
        var currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
        currentActorAccessor.GetCurrent().Returns(CurrentActor.Anonymous);

        var handler = new GetCustomerProfileHandler(queryService, currentActorAccessor);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.Handle(new GetCustomerProfileQuery(), CancellationToken.None).AsTask());
    }
}
