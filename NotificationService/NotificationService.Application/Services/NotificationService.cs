using AutoMapper;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Infrastructure.Interfaces;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Notification> Create(CreateNotificationModel model, string externalId, CancellationToken cancellationToken)
    {
        var notification = _mapper.Map<Notification>(model);

        notification.ExternalId = externalId;

        await _repository.Create(notification, cancellationToken);

        return notification;
    }

    public async Task<Notification> GetById(string id, string requestingExternalId, bool isAdmin, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetById(id, cancellationToken)
            ?? throw new NotificationNotFoundException(id);

        if (!isAdmin && notification.ExternalId != requestingExternalId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this notification.");
        }

        return notification;
    }

    public async Task Update(string id, string requestingExternalId, bool isAdmin, UpdateNotificationModel model, CancellationToken cancellationToken)
    {
        var existingNotification = await GetById(id, requestingExternalId, isAdmin, cancellationToken);

        existingNotification.Title = model.Title;
        existingNotification.Message = model.Message;

        await _repository.Update(existingNotification, cancellationToken);
    }

    public async Task Delete(string id, string requestingExternalId, bool isAdmin, CancellationToken cancellationToken)
    {
        await GetById(id, requestingExternalId, isAdmin, cancellationToken);

        await _repository.Delete(id, cancellationToken);
    }

    public async Task<IList<Notification>> GetByUserPaged(Guid userId, string requestingExternalId, bool isAdmin, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetByUserIdPaged(userId, page, pageSize, cancellationToken);

        if (notifications.Count == 0)
        {
            return notifications;
        }

        var ownerExternalId = notifications[0].ExternalId;

        if (!isAdmin && ownerExternalId != requestingExternalId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own these notifications.");
        }

        return notifications;
    }
}
