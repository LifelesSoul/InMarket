using Microsoft.Extensions.Options;
using ProductService.API.Configurations;
using ProductService.API.Extensions;
using Shouldly;
using System.Security.Claims;
using Xunit;

namespace ProductService.Tests.Extensions;

public class KeycloakRoleTransformationTests
{
    private const string KeycloakIssuer = "http://localhost:8080/realms/inmarket-realm";
    private const string Auth0Issuer = "https://dev-lifelesssoul.eu.auth0.com/";
    private const string AdminAndOffline = """{"roles":["Admin","offline_access"]}""";

    private readonly KeycloakRoleTransformation _transformation =
        new(Options.Create(new KeycloakSettings { Authority = KeycloakIssuer }));

    [Fact]
    public async Task TransformAsync_WhenKeycloakTokenHasRealmRoles_AddsRoleClaims()
    {
        var principal = CreatePrincipal(KeycloakIssuer, AdminAndOffline);

        var result = await _transformation.TransformAsync(principal);

        result.IsInRole("Admin").ShouldBeTrue();
        result.IsInRole("offline_access").ShouldBeTrue();
        result.Identity!.IsAuthenticated.ShouldBeTrue();
        principal.IsInRole("Admin").ShouldBeFalse();
    }

    [Fact]
    public async Task TransformAsync_WhenRunTwice_DoesNotDuplicateRoles()
    {
        var principal = CreatePrincipal(KeycloakIssuer, AdminAndOffline);

        var first = await _transformation.TransformAsync(principal);
        var second = await _transformation.TransformAsync(first);

        second.ShouldBeSameAs(first);
        second.FindAll(ClaimTypes.Role).Count(claim => claim.Value == "Admin").ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"roles":"Admin"}""")]
    [InlineData("""{"roles":[1,null,""]}""")]
    public async Task TransformAsync_WhenRealmAccessIsMissingOrMalformed_AddsNoRoles(string? realmAccess)
    {
        var principal = CreatePrincipal(KeycloakIssuer, realmAccess);

        var result = await _transformation.TransformAsync(principal);

        result.ShouldBeSameAs(principal);
        result.FindAll(ClaimTypes.Role).ShouldBeEmpty();
    }

    [Fact]
    public async Task TransformAsync_WhenTokenIsFromAuth0_LeavesPrincipalUntouched()
    {
        var principal = CreatePrincipal(Auth0Issuer, AdminAndOffline);

        var result = await _transformation.TransformAsync(principal);

        result.ShouldBeSameAs(principal);
        result.IsInRole("Admin").ShouldBeFalse();
    }

    [Fact]
    public async Task TransformAsync_WhenUserIsNotAuthenticated_LeavesPrincipalUntouched()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new("iss", KeycloakIssuer),
            new("realm_access", AdminAndOffline)
        }));

        var result = await _transformation.TransformAsync(principal);

        result.ShouldBeSameAs(principal);
        result.IsInRole("Admin").ShouldBeFalse();
    }

    private static ClaimsPrincipal CreatePrincipal(string issuer, string? realmAccess)
    {
        var claims = new List<Claim>
        {
            new("iss", issuer),
            new(ClaimTypes.NameIdentifier, "user-1")
        };

        if (realmAccess is not null)
        {
            claims.Add(new Claim("realm_access", realmAccess, "JSON"));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer"));
    }
}
