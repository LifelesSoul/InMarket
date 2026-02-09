using AutoMapper;
using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Application.Services;

public class NotificationCreateConsumer(
    INotificationService notificationService,
    IMapper mapper
    ) : IConsumer<CreateNotificationEvent>
{
    public async Task Consume(ConsumeContext<CreateNotificationEvent> context)
    {
        var message = context.Message;
        var createModel = mapper.Map<CreateNotificationModel>(message);

        var ownerExternalId = message.ExternalId;

        await notificationService.Create(createModel, ownerExternalId, context.CancellationToken);
    }
}
