using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace ProductService.API.Extensions;

[ExcludeFromCodeCoverage]
public static class ClaimsPrincipalExtensions
{
    public static string GetExternalId(this ClaimsPrincipal principal)
    {
        var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(externalId))
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }

        return externalId;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Admin");
    }
}
