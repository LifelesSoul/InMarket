using Microsoft.AspNetCore.Authorization;
using NotificationService.API.Extensions;
using NotificationService.Domain.Entities;

namespace NotificationService.Authorization;

public class SameAuthorRequirement : IAuthorizationRequirement { }

public class NotificationAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Notification>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameAuthorRequirement requirement,
        Notification resource)
    {
        if (context.User.IsAdmin())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId != null && resource.ExternalId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
