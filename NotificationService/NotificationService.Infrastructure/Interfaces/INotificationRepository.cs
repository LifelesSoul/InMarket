using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Interfaces;

public interface INotificationRepository
{
    Task Create(Notification notification);
    Task<List<Notification>> GetByUserId(Guid userId);
    Task<Notification?> GetById(Guid id);
    Task<List<Notification>> GetAllPagedAsync(int page, int pageSize);
    Task Update(Notification notification);
    Task Delete(Guid id);
}
