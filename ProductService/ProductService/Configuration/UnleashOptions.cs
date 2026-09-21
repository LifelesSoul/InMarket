using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Configurations;

[ExcludeFromCodeCoverage]
public class UnleashOptions
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
}
