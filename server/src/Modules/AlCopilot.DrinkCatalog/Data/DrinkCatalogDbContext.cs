using System.Text.Json;
using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Data;

public sealed class DrinkCatalogDbContext(DbContextOptions<DrinkCatalogDbContext> options) : DbContext(options)
{
    private static readonly DomainEventTypeRegistry EventTypeRegistry =
        DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly);

    public DbSet<Drink> Drinks => Set<Drink>();
    public DbSet<RecipeEntry> RecipeEntries => Set<RecipeEntry>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<IngredientCategory> IngredientCategories => Set<IngredientCategory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<DomainEventRecord> DomainEventRecords => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("drink_catalog");

        modelBuilder.Entity<Drink>(builder =>
        {
            builder.ToTable("Drinks");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<RecipeEntry>(builder =>
        {
            builder.ToTable("RecipeEntries");
            builder.HasKey(x => new { x.DrinkId, x.IngredientId });
            builder.Property(x => x.Quantity).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RecommendedBrand).HasMaxLength(200);
            builder.HasOne<Drink>().WithMany(x => x.RecipeEntries).HasForeignKey(x => x.DrinkId);
            builder.HasOne<Ingredient>().WithMany().HasForeignKey(x => x.IngredientId);
        });

        modelBuilder.Entity<Ingredient>(builder =>
        {
            builder.ToTable("Ingredients");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.NotableBrands).HasColumnType("jsonb");
            builder.HasOne<IngredientCategory>().WithMany().HasForeignKey(x => x.IngredientCategoryId);
        });

        modelBuilder.Entity<IngredientCategory>(builder =>
        {
            builder.ToTable("IngredientCategories");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Tag>(builder =>
        {
            builder.ToTable("Tags");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Drink>()
            .HasMany(x => x.Tags)
            .WithMany(x => x.Drinks)
            .UsingEntity(join => join.ToTable("DrinkTag", "drink_catalog"));

        modelBuilder.Entity<DomainEventRecord>(builder =>
        {
            builder.ToTable("domain_events");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AggregateType).HasMaxLength(200).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            builder.HasIndex(x => x.OccurredAtUtc);
            builder.HasIndex(x => new { x.AggregateId, x.Id });
            builder.HasIndex(x => new { x.DispatchedAtUtc, x.Id })
                .HasFilter("\"DispatchedAtUtc\" IS NULL");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PersistDomainEventsToOutbox();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        PersistDomainEventsToOutbox();
        return base.SaveChanges();
    }

    private void PersistDomainEventsToOutbox()
    {
        var trackedDrinks = ChangeTracker
            .Entries<Drink>()
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var drink in trackedDrinks)
        {
            foreach (var domainEvent in drink.DequeueDomainEvents())
            {
                DomainEventRecords.Add(new DomainEventRecord
                {
                    AggregateId = domainEvent.AggregateId,
                    AggregateType = nameof(Drink),
                    EventType = EventTypeRegistry.GetName(domainEvent.GetType()),
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredAtUtc = domainEvent.OccurredAtUtc
                });
            }
        }
    }
}
