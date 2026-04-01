using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
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

        ConfigureDrink(modelBuilder);
        ConfigureTag(modelBuilder);
        ConfigureIngredientCategory(modelBuilder);
        ConfigureIngredient(modelBuilder);
        ConfigureRecipeEntry(modelBuilder);
        ConfigureDomainEventRecord(modelBuilder);
    }

    private static void ConfigureDrink(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Drink>(drink =>
        {
            drink.HasKey(d => d.Id);
            drink.Property(d => d.Name)
                .HasConversion(v => v.Value, raw => DrinkName.Create(raw))
                .IsRequired()
                .HasMaxLength(200);
            drink.Property(d => d.Description).HasMaxLength(2000);
            drink.Property(d => d.ImageUrl)
                .HasConversion(v => v.Value, raw => ImageUrl.Create(raw))
                .IsRequired(false)
                .HasMaxLength(1000);
            drink.HasIndex(d => d.Name).IsUnique();
            drink.HasQueryFilter(d => !d.IsDeleted);

            drink.HasMany(d => d.Tags)
                .WithMany(t => t.Drinks)
                .UsingEntity("DrinkTag",
                    l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagId"),
                    r => r.HasOne(typeof(Drink)).WithMany().HasForeignKey("DrinkId"));

            drink.HasMany(d => d.RecipeEntries)
                .WithOne()
                .HasForeignKey(e => e.DrinkId)
                .OnDelete(DeleteBehavior.Cascade);

            drink.Navigation(d => d.Tags).AutoInclude();
            drink.Navigation(d => d.RecipeEntries).AutoInclude();

            drink.Ignore(d => d.DomainEvents);
        });
    }

    private static void ConfigureTag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(tag =>
        {
            tag.HasKey(t => t.Id);
            tag.Property(t => t.Name)
                .HasConversion(v => v.Value, raw => TagName.Create(raw))
                .IsRequired()
                .HasMaxLength(100);
            tag.HasIndex(t => t.Name).IsUnique();
            tag.Ignore(t => t.DomainEvents);
        });
    }

    private static void ConfigureIngredientCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngredientCategory>(category =>
        {
            category.HasKey(c => c.Id);
            category.Property(c => c.Name)
                .HasConversion(v => v.Value, raw => CategoryName.Create(raw))
                .IsRequired()
                .HasMaxLength(100);
            category.HasIndex(c => c.Name).IsUnique();
            category.Ignore(c => c.DomainEvents);
        });
    }

    private static void ConfigureIngredient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>(ingredient =>
        {
            ingredient.HasKey(i => i.Id);
            ingredient.Property(i => i.Name)
                .HasConversion(v => v.Value, raw => IngredientName.Create(raw))
                .IsRequired()
                .HasMaxLength(200);
            ingredient.HasIndex(i => i.Name).IsUnique();
            ingredient.Property(i => i.NotableBrands).HasColumnType("jsonb");
            ingredient.HasOne<IngredientCategory>()
                .WithMany()
                .HasForeignKey(i => i.IngredientCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            ingredient.Ignore(i => i.DomainEvents);
        });
    }

    private static void ConfigureRecipeEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeEntry>(entry =>
        {
            entry.HasKey(e => new { e.DrinkId, e.IngredientId });
            entry.Property(e => e.Quantity)
                .HasConversion(v => v.Value, raw => Quantity.Create(raw))
                .IsRequired()
                .HasMaxLength(100);
            entry.Property(e => e.RecommendedBrand).HasMaxLength(200);
            entry.HasOne<Ingredient>()
                .WithMany()
                .HasForeignKey(e => e.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDomainEventRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainEventRecord>(record =>
        {
            record.ToTable("domain_events", "drink_catalog");
            record.HasKey(r => r.Id);
            record.Property(r => r.Id).UseIdentityByDefaultColumn();
            record.Property(r => r.AggregateType).IsRequired().HasMaxLength(200);
            record.Property(r => r.EventType).IsRequired().HasMaxLength(500);
            record.Property(r => r.Payload).IsRequired().HasColumnType("jsonb");
            record.HasIndex(r => r.IsPublished).HasFilter("\"IsPublished\" = false");
        });
    }
}
