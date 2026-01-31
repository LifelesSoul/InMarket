using MassTransit;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Events;

[ExcludeFromCodeCoverage]
public static class NotificationConfig
{
    public const string ExchangeName = "notification-create";
}

[ExcludeFromCodeCoverage]
[EntityName(NotificationConfig.ExchangeName)]
[MessageUrn(NotificationConfig.ExchangeName)]
public record CreateNotificationEvent
{
    public required string Title { get; init; }

    public required string Message { get; init; }

    public required Guid UserId { get; init; }

    public string? ExternalId { get; init; }
}
