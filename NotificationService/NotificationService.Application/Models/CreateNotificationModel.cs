namespace NotificationService.Application.Models;

public record CreateNotificationModel(string Title, string Message, Guid UserId);
