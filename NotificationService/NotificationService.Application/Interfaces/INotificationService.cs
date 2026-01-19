using NotificationService.Application.Models;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> Create(CreateNotificationModel model);
    Task<Notification> GetById(Guid id);
    Task Update(Guid id, UpdateNotificationModel model);
    Task Delete(Guid id);
    Task<List<Notification>> GetAllPaged(int page, int pageSize);
    Task<List<Notification>> GetByUser(Guid userId);
}
