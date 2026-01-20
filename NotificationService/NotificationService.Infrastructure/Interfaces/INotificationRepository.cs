using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Interfaces;

public interface INotificationRepository
{
    Task Create(Notification notification, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByUserIdPaged(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Notification?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(Notification notification, CancellationToken cancellationToken);
    Task Delete(Guid id, CancellationToken cancellationToken);
}
