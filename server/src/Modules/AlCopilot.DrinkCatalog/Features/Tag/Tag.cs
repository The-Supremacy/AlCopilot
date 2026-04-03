namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public List<AlCopilot.DrinkCatalog.Features.Drink.Drink> Drinks { get; set; } = [];
}
