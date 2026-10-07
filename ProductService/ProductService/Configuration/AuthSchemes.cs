using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Configurations;

[ExcludeFromCodeCoverage]
public static class AuthSchemes
{
    public const string Auth0 = "Auth0";

    public const string Keycloak = "Keycloak";

    public const string Selector = "Auth0OrKeycloak";
}
