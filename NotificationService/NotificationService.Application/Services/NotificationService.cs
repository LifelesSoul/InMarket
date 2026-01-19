using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Interfaces;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Notification> Create(CreateNotificationModel model)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Message = model.Message,
            UserId = model.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.Create(notification);

        return notification;
    }

    public async Task<Notification> GetById(Guid id)
    {
        var notification = await _repository.GetById(id)
            ?? throw new KeyNotFoundException($"Notification with ID {id} not found");

        return notification;
    }

    public async Task Update(Guid id, UpdateNotificationModel model)
    {
        var existingNotification = await GetById(id);

        existingNotification.Title = model.Title;
        existingNotification.Message = model.Message;

        await _repository.Update(existingNotification);
    }

    public async Task Delete(Guid id)
    {
        await GetById(id);

        await _repository.Delete(id);
    }

    public async Task<List<Notification>> GetAllPaged(int page, int pageSize)
    {
        return await _repository.GetAllPagedAsync(page, pageSize);
    }

    public async Task<List<Notification>> GetByUser(Guid userId)
    {
        return await _repository.GetByUserId(userId);
    }
}
