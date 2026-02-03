namespace NotificationService.API.Configurations;

public record RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public required string ConnectionString { get; init; }

    public required QueueSettings ProductCreated { get; init; }
}

public class QueueSettings
{
    public required string QueueName { get; init; }

    public required string ExchangeName { get; init; }
}
