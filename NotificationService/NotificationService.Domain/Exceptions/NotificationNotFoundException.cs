namespace NotificationService.Domain.Exceptions;

public class NotificationNotFoundException : Exception
{
    public NotificationNotFoundException(Guid id)
        : base($"Notification with ID '{id}' was not found.")
    {
    }
}
