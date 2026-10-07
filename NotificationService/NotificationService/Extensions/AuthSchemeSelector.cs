using Microsoft.IdentityModel.JsonWebTokens;
using NotificationService.API.Configuration;
using NotificationService.Application.Constants;

namespace NotificationService.API.Extensions;

public static class AuthSchemeSelector
{
    private const string BearerPrefix = "Bearer ";
    private const string AccessTokenQuery = "access_token";

    public static string ForRequest(HttpContext context, string keycloakIssuer)
    {
        return ForToken(ReadBearerToken(context) ?? ReadHubToken(context), keycloakIssuer);
    }

    public static string? ReadHubToken(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(HubRoutes.NotificationHub))
        {
            return null;
        }

        var token = context.Request.Query[AccessTokenQuery].ToString();

        return string.IsNullOrEmpty(token) ? null : token;
    }

    public static string ForToken(string? token, string keycloakIssuer)
    {
        var handler = new JsonWebTokenHandler();

        if (string.IsNullOrEmpty(token) || !handler.CanReadToken(token))
        {
            return AuthConstants.Auth0Scheme;
        }

        try
        {
            var issuer = handler.ReadJsonWebToken(token).Issuer;

            return SameIssuer(issuer, keycloakIssuer) ? AuthConstants.KeycloakScheme : AuthConstants.Auth0Scheme;
        }
        catch (ArgumentException)
        {
            return AuthConstants.Auth0Scheme;
        }
    }

    private static string? ReadBearerToken(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();

        return header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? header[BearerPrefix.Length..].Trim()
            : null;
    }

    private static bool SameIssuer(string? issuer, string expected)
    {
        return issuer is not null
            && string.Equals(issuer.TrimEnd('/'), expected.TrimEnd('/'), StringComparison.Ordinal);
    }
}
