using NotificationService.Application.Models;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> Create(CreateNotificationModel model, CancellationToken cancellationToken);
    Task<Notification> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(Guid id, UpdateNotificationModel model, CancellationToken cancellationToken);
    Task Delete(Guid id, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByUserPaged(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
