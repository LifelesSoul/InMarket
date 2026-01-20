using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreateNotificationModel model,
    CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        model.UserId = userId;

        var createdNotification = await _service.Create(model, cancellationToken);

        return Ok(createdNotification);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetByUser(
    [FromQuery] PaginationParams pagination,
    CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var notifications = await _service.GetByUserPaged(
            userId,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var notification = await _service.GetById(id, cancellationToken);
        return Ok(notification);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, CancellationToken cancellationToken, [FromBody] UpdateNotificationModel dto)
    {
        await _service.Update(id, dto, cancellationToken);

        var updated = await _service.GetById(id, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _service.Delete(id, cancellationToken);
        return NoContent();
    }
}
