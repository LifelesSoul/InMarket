using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NotificationService.API.Extensions;

public static class ControllerExtensions
{
    public static string GetExternalUserId(this ControllerBase controller)
    {
        var externalId = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID is missing in token.");

        return externalId;
    }
}
