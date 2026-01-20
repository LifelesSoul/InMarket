using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Interfaces;

public interface INotificationRepository
{
    Task Create(Notification notification, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByUserIdPaged(string userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Notification?> GetById(string id, CancellationToken cancellationToken);
    Task Update(Notification notification, CancellationToken cancellationToken);
    Task Delete(string id, CancellationToken cancellationToken);
}
