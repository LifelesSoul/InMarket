namespace NotificationService.Application.Models;

public class CreateNotificationModel
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required Guid UserId { get; set; }
    public string? ExternalId { get; set; }
}
