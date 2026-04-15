using AlCopilot.CustomerProfile.Contracts.Commands;
using AlCopilot.CustomerProfile.Features.Profile;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.CustomerProfile.Tests.Handlers;

public sealed class SaveCustomerProfileHandlerTests
{
    private readonly ICustomerProfileRepository _repository = Substitute.For<ICustomerProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentActorAccessor _currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
    private readonly SaveCustomerProfileHandler _handler;

    public SaveCustomerProfileHandlerTests()
    {
        _currentActorAccessor.GetCurrent().Returns(new CurrentActor("customer-1", "customer@example.com", true, ["user"]));
        _handler = new SaveCustomerProfileHandler(_repository, _unitOfWork, _currentActorAccessor);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_CreatesAndNormalizesSets()
    {
        _repository.GetByCustomerIdAsync("customer-1", Arg.Any<CancellationToken>())
            .Returns((CustomerProfile.Features.Profile.CustomerProfile?)null);

        var response = await _handler.Handle(
            new SaveCustomerProfileCommand(
                [Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Empty, Guid.Parse("00000000-0000-0000-0000-000000000001")],
                [Guid.Parse("00000000-0000-0000-0000-000000000004"), Guid.Parse("00000000-0000-0000-0000-000000000004")],
                [],
                [Guid.Parse("00000000-0000-0000-0000-000000000003")]),
            CancellationToken.None);

        response.FavoriteIngredientIds.ShouldBe(
            [Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("00000000-0000-0000-0000-000000000002")]);
        response.DislikedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000004")]);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProfileExists_UpdatesExistingProfile()
    {
        var existing = CustomerProfile.Features.Profile.CustomerProfile.Create(CustomerIdentity.Create("customer-1"));
        _repository.GetByCustomerIdAsync("customer-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var response = await _handler.Handle(
            new SaveCustomerProfileCommand(
                [],
                [Guid.Parse("00000000-0000-0000-0000-000000000005")],
                [Guid.Parse("00000000-0000-0000-0000-000000000006")],
                [Guid.Parse("00000000-0000-0000-0000-000000000007")]),
            CancellationToken.None);

        response.DislikedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000005")]);
        response.ProhibitedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000006")]);
        response.OwnedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000007")]);
    }

    [Fact]
    public async Task Handle_Throws_WhenActorIsAnonymous()
    {
        _currentActorAccessor.GetCurrent().Returns(CurrentActor.Anonymous);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(
                new SaveCustomerProfileCommand([], [], [], []),
                CancellationToken.None).AsTask());
    }
}
