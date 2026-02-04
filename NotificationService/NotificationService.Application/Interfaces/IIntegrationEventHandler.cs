namespace NotificationService.Application.Interfaces;

public interface IIntegrationEventHandler<in TEvent>
{
    Task Handle(TEvent @event);
}
