using System.Security.Claims;

namespace Backend.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue(ClaimTypes.Name)
                  ?? principal.FindFirstValue(ClaimTypes.Sid)
                  ?? principal.FindFirstValue("sub");

        return Guid.TryParse(raw, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User id claim is missing.");
    }
}
