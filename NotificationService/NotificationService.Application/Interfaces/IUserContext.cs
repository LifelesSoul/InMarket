namespace NotificationService.Application.Interfaces;

public interface IUserContext
{
    string ExternalId { get; }

    bool IsAdmin { get; }

    bool IsOwner(string resourceOwnerId);
}
