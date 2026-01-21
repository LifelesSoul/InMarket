using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Configuration;
using NotificationService.API.Extensions;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Infrastructure.Models;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly IAuthorizationService _authService;

    public NotificationController(INotificationService service, IAuthorizationService authService)
    {
        _service = service;
        _authService = authService;
    }

    [HttpPost]
    [Authorize(Roles = AuthConstants.AdminRole)]
    public async Task<IActionResult> Create([FromBody] CreateNotificationModel dto, CancellationToken cancellationToken)
    {
        var ownerId = !string.IsNullOrEmpty(dto.ExternalId)
            ? dto.ExternalId
            : User.GetExternalId();

        var createdNotification = await _service.Create(dto, ownerId, cancellationToken);

        return Ok(createdNotification);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetByUser(
        [FromQuery] Guid userId,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var filter = new NotificationFilter
        {
            UserId = userId,
            ExternalId = User.IsAdmin() ? null : User.GetExternalId()
        };

        var notifications = await _service.GetByFilter(
            filter,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var notification = await _service.GetById(id, cancellationToken);

        var authResult = await _authService.AuthorizeAsync(User, notification, AuthConstants.OwnerPolicyName);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(notification);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AuthConstants.AdminRole)]
    public async Task<IActionResult> Update(string id, CancellationToken cancellationToken, [FromBody] UpdateNotificationModel dto)
    {
        await _service.Update(id, dto, cancellationToken);

        var updated = await _service.GetById(id, cancellationToken);

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var notification = await _service.GetById(id, cancellationToken);

        var authResult = await _authService.AuthorizeAsync(User, notification, AuthConstants.OwnerPolicyName);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await _service.Delete(id, cancellationToken);

        return NoContent();
    }
}
