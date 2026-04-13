namespace AlCopilot.Shared.Models;

public sealed record CurrentActor(
    string? UserId,
    string DisplayName,
    bool IsAuthenticated)
{
    public static CurrentActor Anonymous { get; } = new(null, "anonymous", false);
}
