using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using NotificationService.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace NotificationService.API.Extensions;

public sealed class KeycloakRoleTransformation(IOptions<KeycloakSettings> settings) : IClaimsTransformation
{
    private const string IssuerClaim = "iss";
    private const string RealmAccessClaim = "realm_access";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } || !IsKeycloak(principal))
        {
            return Task.FromResult(principal);
        }

        var missingRoles = ReadRealmRoles(principal.FindFirst(RealmAccessClaim)?.Value)
            .Where(role => !principal.IsInRole(role))
            .ToList();

        if (missingRoles.Count == 0)
        {
            return Task.FromResult(principal);
        }

        var roles = new ClaimsIdentity();

        foreach (var role in missingRoles)
        {
            roles.AddClaim(new Claim(roles.RoleClaimType, role));
        }

        var transformed = principal.Clone();
        transformed.AddIdentity(roles);

        return Task.FromResult(transformed);
    }

    private bool IsKeycloak(ClaimsPrincipal principal)
    {
        var issuer = principal.FindFirst(IssuerClaim)?.Value;

        return issuer is not null
            && string.Equals(issuer.TrimEnd('/'), settings.Value.Authority.TrimEnd('/'), StringComparison.Ordinal);
    }

    private static List<string> ReadRealmRoles(string? realmAccess)
    {
        if (string.IsNullOrEmpty(realmAccess))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccess);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("roles", out var roles)
                || roles.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return roles.EnumerateArray()
                .Where(role => role.ValueKind == JsonValueKind.String)
                .Select(role => role.GetString()!)
                .Where(role => role.Length > 0)
                .Distinct()
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
