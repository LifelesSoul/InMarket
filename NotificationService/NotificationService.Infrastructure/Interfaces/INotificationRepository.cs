using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Models;

namespace NotificationService.Infrastructure.Interfaces;

public interface INotificationRepository
{
    Task Create(Notification notification, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByFilter(NotificationFilter filter, int page, int pageSize, CancellationToken cancellationToken);
    Task<Notification?> GetById(string id, CancellationToken cancellationToken);
    Task Update(Notification notification, CancellationToken cancellationToken);
    Task Delete(string id, CancellationToken cancellationToken);
}
