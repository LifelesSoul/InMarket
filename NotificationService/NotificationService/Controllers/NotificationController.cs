using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using System.Security.Claims;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationModel dto, CancellationToken cancellationToken)
    {
        var createdNotification = await _service.Create(dto, dto.ExternalId, cancellationToken);

        return Ok(createdNotification);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetByUser(
    [FromQuery] Guid userId,
    [FromQuery] PaginationParams pagination,
    CancellationToken cancellationToken)
    {
        var requestingExternalId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var isAdmin = User.IsInRole("Admin");

        var notifications = await _service.GetByUserPaged(
            userId,
            requestingExternalId!,
            isAdmin,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var requestingExternalId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var isAdmin = User.IsInRole("Admin");

        var notification = await _service.GetById(id, requestingExternalId!, isAdmin, cancellationToken);

        return Ok(notification);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, CancellationToken cancellationToken, [FromBody] UpdateNotificationModel dto)
    {
        var requestingExternalId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var isAdmin = true;

        await _service.Update(id, requestingExternalId!, isAdmin, dto, cancellationToken);

        var updated = await _service.GetById(id, requestingExternalId!, isAdmin, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var requestingExternalId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        await _service.Delete(id, requestingExternalId!, isAdmin, cancellationToken);

        return NoContent();
    }
}
