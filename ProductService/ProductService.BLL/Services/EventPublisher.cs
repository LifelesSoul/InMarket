using MassTransit;
using ProductService.BLL.Events;

namespace ProductService.BLL.Services;

public interface IEventPublisher
{
    Task PublishNotification(CreateNotificationEvent notificationEvent);
}

public class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishNotification(CreateNotificationEvent notificationEvent)
    {
        await publishEndpoint.Publish(notificationEvent);
    }
}
