using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Infrastructure.Interfaces;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Notification> Create(CreateNotificationModel model, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Title = model.Title,
            Message = model.Message,
            UserId = model.UserId,
            CreatedAt = TimeProvider.System.GetUtcNow()
        };

        await _repository.Create(notification, cancellationToken);

        return notification;
    }

    public async Task<Notification> GetById(string id, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetById(id, cancellationToken)
            ?? throw new NotificationNotFoundException(id);

        return notification;
    }

    public async Task Update(string id, UpdateNotificationModel model, CancellationToken cancellationToken)
    {
        var existingNotification = await GetById(id, cancellationToken);

        existingNotification.Title = model.Title;
        existingNotification.Message = model.Message;

        await _repository.Update(existingNotification, cancellationToken);
    }

    public async Task Delete(string id, CancellationToken cancellationToken)
    {
        await GetById(id, cancellationToken);

        await _repository.Delete(id, cancellationToken);
    }

    public async Task<IList<Notification>> GetByUserPaged(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByUserIdPaged(userId, page, pageSize, cancellationToken);
    }
}
