using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Interfaces;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<Notification>(settings.CollectionName);
    }

    public async Task Create(Notification notification, CancellationToken cancellationToken) =>
          await _collection.InsertOneAsync(notification, null, cancellationToken);

    public async Task<IList<Notification>> GetByUserIdPaged(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var filter = Builders<Notification>.Filter.Eq(x => x.UserId, userId);

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetById(Guid id, CancellationToken cancellationToken) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task Update(Notification notification, CancellationToken cancellationToken) =>
        await _collection.ReplaceOneAsync(x => x.Id == notification.Id, notification, (ReplaceOptions?)null, cancellationToken);

    public async Task Delete(Guid id, CancellationToken cancellationToken) =>
        await _collection.DeleteOneAsync(x => x.Id == id, null, cancellationToken);
}
