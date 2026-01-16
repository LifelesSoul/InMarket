namespace ProductService.API.Models.Webhook;

public class Auth0UserDto
{
    public required string Email { get; set; }
    public required string ExternalId { get; set; }
}
