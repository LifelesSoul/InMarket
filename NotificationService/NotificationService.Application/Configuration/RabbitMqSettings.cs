namespace NotificationService.Application.Configurations;

public record RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public required string Host { get; init; }

    public required string VirtualHost { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string QueueName { get; init; }

    public required int RetryCount { get; init; }

    public required int RetryIntervalSeconds { get; init; }
}
