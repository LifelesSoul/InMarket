using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Configurations;

[ExcludeFromCodeCoverage]
public class WebhookSettings
{
    public string ApiKey { get; set; } = string.Empty;
}
