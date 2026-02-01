namespace ProductService.DAL.Settings;

public record RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public required string Host { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string VirtualHost { get; set; }
}
