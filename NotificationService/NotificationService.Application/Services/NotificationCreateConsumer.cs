using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Applcaition.Hubs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;

namespace NotificationService.Application.Services;

public class NotificationCreateConsumer(
    INotificationService notificationService,
    IMapper mapper,
    IHubContext<NotificationHub, INotificationClient> hubContext
    ) : IConsumer<CreateNotificationEvent>
{
    public async Task Consume(ConsumeContext<CreateNotificationEvent> context)
    {
        var message = context.Message;
        var createModel = mapper.Map<CreateNotificationModel>(message);

        var ownerExternalId = message.ExternalId;

        await notificationService.Create(createModel, ownerExternalId, context.CancellationToken);

        if (!string.IsNullOrEmpty(ownerExternalId))
        {
            await hubContext.Clients.User(ownerExternalId)
                .ReceiveNotification(message.Message, message.Title);
        }
    }
}
