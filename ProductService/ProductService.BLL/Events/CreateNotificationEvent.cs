namespace ProductService.BLL.Events;

public record CreateNotificationEvent
{
    public required Guid UserId { get; init; }

    public required string Title { get; init; }

    public required string Message { get; init; }

    public required Guid ProductId { get; init; }
}
