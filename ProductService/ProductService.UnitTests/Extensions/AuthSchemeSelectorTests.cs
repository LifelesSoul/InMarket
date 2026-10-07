using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using ProductService.API.Configurations;
using ProductService.API.Extensions;
using Shouldly;
using Xunit;

namespace ProductService.Tests.Extensions;

public class AuthSchemeSelectorTests
{
    private const string KeycloakIssuer = "http://localhost:8080/realms/inmarket-realm";
    private const string Auth0Issuer = "https://dev-lifelesssoul.eu.auth0.com/";

    [Fact]
    public void ForToken_WhenIssuerIsKeycloak_ReturnsKeycloak()
    {
        AuthSchemeSelector.ForToken(CreateToken(KeycloakIssuer), KeycloakIssuer).ShouldBe(AuthSchemes.Keycloak);
    }

    [Fact]
    public void ForToken_WhenIssuerIsAuth0_ReturnsAuth0()
    {
        AuthSchemeSelector.ForToken(CreateToken(Auth0Issuer), KeycloakIssuer).ShouldBe(AuthSchemes.Auth0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("a.b.c")]
    public void ForToken_WhenTokenIsMissingOrBroken_ReturnsAuth0(string? token)
    {
        AuthSchemeSelector.ForToken(token, KeycloakIssuer).ShouldBe(AuthSchemes.Auth0);
    }

    [Fact]
    public void ForRequest_ReadsTheBearerToken()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {CreateToken(KeycloakIssuer)}";

        AuthSchemeSelector.ForRequest(context, KeycloakIssuer).ShouldBe(AuthSchemes.Keycloak);
    }

    [Fact]
    public void ForRequest_WhenThereIsNoToken_ReturnsAuth0()
    {
        AuthSchemeSelector.ForRequest(new DefaultHttpContext(), KeycloakIssuer).ShouldBe(AuthSchemes.Auth0);
    }

    private static string CreateToken(string issuer)
    {
        var header = Base64UrlEncoder.Encode("""{"alg":"RS256","typ":"JWT"}""");
        var payload = Base64UrlEncoder.Encode($$"""{"iss":"{{issuer}}","sub":"user-1"}""");

        return $"{header}.{payload}.c2lnbmF0dXJl";
    }
}
