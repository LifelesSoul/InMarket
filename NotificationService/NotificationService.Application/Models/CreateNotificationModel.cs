namespace NotificationService.Application.Models;

public class CreateNotificationModel
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string UserId { get; set; }
}
