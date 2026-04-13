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

        var userId = user.FindFirstValue("sub");
        var displayName = user.FindFirstValue("preferred_username")
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.Identity?.Name
            ?? "authenticated-user";

        return new CurrentActor(userId, displayName, true);
    }
}
