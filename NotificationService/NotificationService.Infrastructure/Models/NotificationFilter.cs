namespace NotificationService.Infrastructure.Models;

public class NotificationFilter
{
    public Guid UserId { get; set; }
    public string? ExternalId { get; set; }
}
