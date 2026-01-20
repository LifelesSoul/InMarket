using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public required Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = TimeProvider.System.GetUtcNow();
}
