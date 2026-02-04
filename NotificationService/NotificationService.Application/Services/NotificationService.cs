using AutoMapper;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Infrastructure.Interfaces;
using NotificationService.Infrastructure.Models;

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

    public async Task<Notification> Create(CreateNotificationModel model, string ownerExternalId, CancellationToken cancellationToken)
    {
        var notification = _mapper.Map<Notification>(model);

        notification.ExternalId = ownerExternalId;

        await _repository.Create(notification, cancellationToken);

        return notification;
    }

    public async Task<Notification> GetById(string id, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetById(id, cancellationToken)
            ?? throw new NotificationNotFoundException(id);

        return notification;
    }

    public async Task Update(string id, UpdateNotificationModel model, CancellationToken cancellationToken)
    {
        var existingNotification = await GetById(id, cancellationToken);

        existingNotification.Title = model.Title;
        existingNotification.Message = model.Message;

        await _repository.Update(existingNotification, cancellationToken);
    }

    public async Task Delete(string id, CancellationToken cancellationToken)
    {
        await GetById(id, cancellationToken);

        await _repository.Delete(id, cancellationToken);
    }

    public async Task<IList<Notification>> GetByFilter(NotificationFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _repository.GetByFilter(filter, page, pageSize, cancellationToken);
    }
}