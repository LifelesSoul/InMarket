using MassTransit;

namespace NotificationService.Application.Models;

public static class NotificationConfig
{
    public const string ExchangeName = "notification-create";
}

[EntityName(NotificationConfig.ExchangeName)]
[MessageUrn(NotificationConfig.ExchangeName)]
public record CreateNotificationEvent
{
    public required string Title { get; init; }

    public required string Message { get; init; }

    public required Guid UserId { get; init; }

    public string? ExternalId { get; init; }
}
