using AlCopilot.CustomerProfile.Data;
using AlCopilot.CustomerProfile.Features.Profile;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.CustomerProfile.IntegrationTests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class CustomerProfileRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private CustomerProfileDbContext _db = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM customer_profile.\"DomainEventRecords\"; DELETE FROM customer_profile.\"CustomerProfiles\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task SaveAndQuery_PersistsProfileByCustomerIdentity()
    {
        var repository = new CustomerProfileRepository(_db);
        var profile = CustomerProfile.Features.Profile.CustomerProfile.Create(CustomerIdentity.Create("customer-1"));
        profile.UpdateIngredientSets(
            [Guid.Parse("00000000-0000-0000-0000-000000000001")],
            [Guid.Parse("00000000-0000-0000-0000-000000000002")],
            [Guid.Parse("00000000-0000-0000-0000-000000000003")],
            [Guid.Parse("00000000-0000-0000-0000-000000000004")]);

        repository.Add(profile);
        await _db.SaveChangesAsync();

        var loaded = await repository.GetByCustomerIdAsync("customer-1");
        loaded.ShouldNotBeNull();
        loaded!.OwnedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000004")]);
        var domainEvents = await _db.DomainEventRecords
            .OrderBy(record => record.Id)
            .ToListAsync();
        domainEvents.Count.ShouldBe(2);
        domainEvents.Select(record => record.EventType)
            .ShouldBe(["customer-profile.profile-created.v1", "customer-profile.profile-updated.v1"]);

        var dto = await new CustomerProfileQueryService(_db).GetByCustomerIdAsync("customer-1");
        dto.ProhibitedIngredientIds.ShouldBe([Guid.Parse("00000000-0000-0000-0000-000000000003")]);
    }
}
