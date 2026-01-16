using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Models.Webhook;

[ExcludeFromCodeCoverage]
public class Auth0UserDto
{
    public required string Email { get; set; }
    public required string ExternalId { get; set; }
}
