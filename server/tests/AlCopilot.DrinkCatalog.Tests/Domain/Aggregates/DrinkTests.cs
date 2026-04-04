using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Domain.Aggregates;

public sealed class DrinkTests
{
    [Fact]
    public void Create_SetsPropertiesAndRaisesEvent()
    {
        var drink = Drink.Create(DrinkName.Create("Margarita"), "A classic cocktail", ImageUrl.Create(null));

        drink.Id.ShouldNotBe(Guid.Empty);
        drink.Name.Value.ShouldBe("Margarita");
        drink.Description.ShouldBe("A classic cocktail");
        drink.IsDeleted.ShouldBeFalse();
        drink.DomainEvents.ShouldHaveSingleItem();
        drink.DomainEvents[0].ShouldBeOfType<DrinkCreatedEvent>();
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), "Old desc", ImageUrl.Create(null));
        drink.ClearDomainEvents();

        drink.Update(DrinkName.Create("New"), "New desc", ImageUrl.Create("https://img.com/new.jpg"));

        drink.Name.Value.ShouldBe("New");
        drink.Description.ShouldBe("New desc");
        drink.ImageUrl.Value.ShouldBe("https://img.com/new.jpg");
    }

    [Fact]
    public void SoftDelete_SetsFlagAndRaisesEvent()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        drink.ClearDomainEvents();

        drink.SoftDelete();

        drink.IsDeleted.ShouldBeTrue();
        drink.DeletedAtUtc.ShouldNotBeNull();
        drink.DomainEvents.ShouldHaveSingleItem();
        drink.DomainEvents[0].ShouldBeOfType<DrinkDeletedEvent>();
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_DoesNothing()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        drink.SoftDelete();
        drink.ClearDomainEvents();

        drink.SoftDelete();

        drink.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void SetTags_ReplacesTags()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        var tag1 = Tag.Create(TagName.Create("Classic"));
        var tag2 = Tag.Create(TagName.Create("Strong"));

        drink.SetTags([tag1, tag2]);

        drink.Tags.Count.ShouldBe(2);
    }

    [Fact]
    public void SetRecipeEntries_ReplacesEntries()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        var entry = RecipeEntry.Create(drink.Id, Guid.NewGuid(), Quantity.Create("2 oz"), null);

        drink.SetRecipeEntries([entry]);

        drink.RecipeEntries.Count.ShouldBe(1);
    }
}
