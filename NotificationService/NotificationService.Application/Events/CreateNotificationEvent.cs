namespace NotificationService.Application.Models.Events;

public class CreateNotificationEvent
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid ProductId { get; set; }
}
