using System.Security.Claims;

namespace NotificationService.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetExternalId(this ClaimsPrincipal principal)
    {
        var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(externalId))
        {
            throw new UnauthorizedAccessException("User ID (sub) claim is missing in the token.");
        }

        return externalId;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Admin");
    }
}
