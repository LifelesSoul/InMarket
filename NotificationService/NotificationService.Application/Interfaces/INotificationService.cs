using NotificationService.Application.Models;
using NotificationService.Application.Models.Events;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Models;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> Create(CreateNotificationModel model, string externalId, CancellationToken cancellationToken);
    Task<Notification> GetById(string id, CancellationToken cancellationToken);
    Task Update(string id, UpdateNotificationModel model, CancellationToken cancellationToken);
    Task Delete(string id, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByFilter(NotificationFilter filter, int page, int pageSize, CancellationToken cancellationToken);
    Task HandleProductCreated(CreateNotificationEvent notificationEvent, CancellationToken cancellationToken);
}
