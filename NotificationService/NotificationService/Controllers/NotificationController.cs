using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

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
    public async Task<IActionResult> Create([FromBody] CreateNotificationModel dto, CancellationToken cancellationToken)
    {
        var createdNotification = await _service.Create(dto, cancellationToken);

        return Ok(createdNotification);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetByUser(
    [FromQuery] Guid userId,
    [FromQuery] PaginationParams pagination,
    CancellationToken cancellationToken)
    {
        var notifications = await _service.GetByUserPaged(
            userId,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _service.GetById(id, cancellationToken);
        return Ok(notification);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken, [FromBody] UpdateNotificationModel dto)
    {
        await _service.Update(id, dto, cancellationToken);

        var updated = await _service.GetById(id, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.Delete(id, cancellationToken);
        return NoContent();
    }
}
