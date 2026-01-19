using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public required Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
