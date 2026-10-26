using Microsoft.IdentityModel.JsonWebTokens;
using ProductService.API.Configurations;

namespace ProductService.API.Extensions;

public static class AuthSchemeSelector
{
    private const string BearerPrefix = "Bearer ";

    public static string ForRequest(HttpContext context, string keycloakIssuer)
    {
        var header = context.Request.Headers.Authorization.ToString();

        var token = header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? header[BearerPrefix.Length..].Trim()
            : null;

        return ForToken(token, keycloakIssuer);
    }

    public static string ForToken(string? token, string keycloakIssuer)
    {
        var handler = new JsonWebTokenHandler();

        if (string.IsNullOrEmpty(token) || !handler.CanReadToken(token))
        {
            return AuthSchemes.Auth0;
        }

        try
        {
            var issuer = handler.ReadJsonWebToken(token).Issuer;

            return SameIssuer(issuer, keycloakIssuer) ? AuthSchemes.Keycloak : AuthSchemes.Auth0;
        }
        catch (ArgumentException)
        {
            return AuthSchemes.Auth0;
        }
    }

    private static bool SameIssuer(string? issuer, string expected)
    {
        return issuer is not null
            && string.Equals(issuer.TrimEnd('/'), expected.TrimEnd('/'), StringComparison.Ordinal);
    }
}
