using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Configurations;

[ExcludeFromCodeCoverage]
public record Auth0Settings
{
    public string Domain { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
}

[ExcludeFromCodeCoverage]
public record KeycloakSettings
{
    public string Authority { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
}
