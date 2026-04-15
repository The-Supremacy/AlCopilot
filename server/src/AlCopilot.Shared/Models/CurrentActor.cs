namespace AlCopilot.Shared.Models;

public sealed record CurrentActor(
    string? UserId,
    string DisplayName,
    bool IsAuthenticated,
    IReadOnlyList<string>? Roles = null)
{
    public IReadOnlyList<string> EffectiveRoles => Roles ?? [];

    public static CurrentActor Anonymous { get; } = new(null, "anonymous", false, []);
}
