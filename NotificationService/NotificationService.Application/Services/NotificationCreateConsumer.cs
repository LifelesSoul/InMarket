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
        var createModel = mapper.Map<CreateNotificationModel>(context.Message);

        var ownerId = context.Message.ExternalId ?? "System";

        await notificationService.Create(createModel, ownerId, context.CancellationToken);
    }
}
