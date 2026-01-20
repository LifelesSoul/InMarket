using NotificationService.Application.Models;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> Create(CreateNotificationModel model, string externalId, CancellationToken cancellationToken);
    Task<Notification> GetById(string id, string requestingExternalId, bool isAdmin, CancellationToken cancellationToken);
    Task Update(string id, string requestingExternalId, bool isAdmin, UpdateNotificationModel model, CancellationToken cancellationToken);
    Task Delete(string id, string requestingExternalId, bool isAdmin, CancellationToken cancellationToken);
    Task<IList<Notification>> GetByUserPaged(Guid userId, string requestingExternalId, bool isAdmin, int page, int pageSize, CancellationToken cancellationToken);
}
