using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Interfaces;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(IConfiguration config)
    {
        var connectionString = config["MongoSettings:ConnectionString"];
        var dbName = config["MongoSettings:DatabaseName"];
        var collectionName = config["MongoSettings:CollectionName"];

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        _collection = database.GetCollection<Notification>(collectionName);
    }

    public async Task<List<Notification>> GetAllPagedAsync(int page, int pageSize)
    {
        return await _collection.Find(_ => true)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task Create(Notification notification) =>
        await _collection.InsertOneAsync(notification);

    public async Task<List<Notification>> GetByUserId(Guid userId) =>
        await _collection.Find(x => x.UserId == userId).ToListAsync();

    public async Task<Notification?> GetById(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task Update(Notification notification) =>
        await _collection.ReplaceOneAsync(x => x.Id == notification.Id, notification);

    public async Task Delete(Guid id) =>
        await _collection.DeleteOneAsync(x => x.Id == id);
}
