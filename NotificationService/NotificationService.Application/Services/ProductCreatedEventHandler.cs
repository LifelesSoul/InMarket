using NotificationService.Application.Interfaces;
using NotificationService.Application.Models.Events;

namespace NotificationService.Application.EventHandlers;

public class ProductCreatedEventHandler(INotificationService service)
    : IIntegrationEventHandler<CreateNotificationEvent>
{
    public async Task Handle(CreateNotificationEvent @event)
    {
        await service.HandleProductCreated(@event, CancellationToken.None);
    }
}
