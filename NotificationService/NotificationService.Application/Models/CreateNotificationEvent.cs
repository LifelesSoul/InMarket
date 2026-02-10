using MassTransit;
using System.Diagnostics.CodeAnalysis;

namespace NotificationService.Application.Models;

[ExcludeFromCodeCoverage]
[EntityName(ExchangeName)]
[MessageUrn(ExchangeName)]
public record CreateNotificationEvent
{
    public const string ExchangeName = "notification-create";

    public required string Title { get; init; }

    public required string Message { get; init; }

    public required Guid UserId { get; init; }

    public required string ExternalId { get; init; }
}
