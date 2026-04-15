using System.Security.Claims;
using AlCopilot.Shared.Models;

namespace AlCopilot.Host.Authentication;

public sealed class HttpContextCurrentActorAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentActorAccessor
{
    public CurrentActor GetCurrent()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated is not true)
        {
            return CurrentActor.Anonymous;
        }

        var userId = user.FindFirstValue("sub")
            ?? user.FindFirstValue("preferred_username")
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.Identity?.Name;
        var displayName = user.FindFirstValue("preferred_username")
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.Identity?.Name
            ?? "authenticated-user";
        var roles = user.Claims
            .Where(claim => string.Equals(claim.Type, "roles", StringComparison.Ordinal) ||
                            string.Equals(claim.Type, ClaimTypes.Role, StringComparison.Ordinal))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        return new CurrentActor(userId, displayName, true, roles);
    }
}
