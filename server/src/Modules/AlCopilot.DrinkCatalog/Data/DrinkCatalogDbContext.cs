using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Data;

public sealed class DrinkCatalogDbContext(DbContextOptions<DrinkCatalogDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Drink> Drinks => Set<Drink>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<IngredientCategory> IngredientCategories => Set<IngredientCategory>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeEntry> RecipeEntries => Set<RecipeEntry>();
    public DbSet<DomainEventRecord> DomainEventRecords => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("drink_catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DrinkCatalogDbContext).Assembly);
    }
}
