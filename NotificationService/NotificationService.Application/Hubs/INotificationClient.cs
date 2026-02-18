namespace NotificationService.Applcaition.Hubs;

public interface INotificationClient
{
    Task ReceiveNotification(string message, string title);
}
