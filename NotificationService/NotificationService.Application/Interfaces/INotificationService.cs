using NotificationService.Application.Models;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> Create(CreateNotificationModel model, CancellationToken cancellationToken);
    Task<Notification> GetById(string id, CancellationToken cancellationToken);
    Task Update(string id, UpdateNotificationModel model, CancellationToken cancellationToken);
    Task Delete(string id, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByUserPaged(string userId, int page, int pageSize, CancellationToken cancellationToken);
}
