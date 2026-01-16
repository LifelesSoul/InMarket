using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.User;

[ExcludeFromCodeCoverage]
public class Auth0SyncModel
{
    public required string ExternalId { get; set; }
    public required string Email { get; set; }
}
