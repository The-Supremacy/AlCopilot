using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class ManagementRepositoryConstraintsIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task TagExistsByNameAsync_ExcludesCurrentTag()
    {
        var tagRepository = new TagRepository(_db);
        var tag = Tag.Create(TagName.Create("Refreshing"));
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        (await tagRepository.ExistsByNameAsync("Refreshing", tag.Id)).ShouldBeFalse();
        (await tagRepository.ExistsByNameAsync("Refreshing")).ShouldBeTrue();
    }

    [Fact]
    public async Task IngredientReferenceCheck_IgnoresSoftDeletedDrinks()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Rum"));
        var drink = Drink.Create(DrinkName.Create("Mai Tai"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("2 oz"), null)]);

        _db.Ingredients.Add(ingredient);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        var ingredientRepository = new IngredientRepository(_db);
        (await ingredientRepository.IsReferencedByActiveDrinksAsync(ingredient.Id)).ShouldBeTrue();

        drink.SoftDelete();
        await _db.SaveChangesAsync();

        (await ingredientRepository.IsReferencedByActiveDrinksAsync(ingredient.Id)).ShouldBeFalse();
    }
}
