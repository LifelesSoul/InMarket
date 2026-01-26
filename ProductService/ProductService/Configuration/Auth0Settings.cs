using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Configurations;

[ExcludeFromCodeCoverage]
public class Auth0Settings
{
    public string Domain { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
