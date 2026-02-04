namespace NotificationService.Application.Models;

public record CreateNotificationModel
{
    public required string Title { get; init; }

    public required string Message { get; init; }

    public required Guid UserId { get; init; }

    public string? ExternalId { get; init; }
}
