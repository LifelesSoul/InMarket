using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProductService.API.Configurations;
using Unleash;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/auth")]
[ExcludeFromCodeCoverage]
public class AuthController : ControllerBase
{
    private const string UseKeycloakAuthFlag = "use-keycloak-auth";

    private readonly IUnleash _unleash;
    private readonly Auth0Settings _auth0Settings;
    private readonly KeycloakSettings _keycloakSettings;

    public AuthController(
        IUnleash unleash,
        IOptions<Auth0Settings> auth0Options,
        IOptions<KeycloakSettings> keycloakOptions)
    {
        _unleash = unleash;
        _auth0Settings = auth0Options.Value;
        _keycloakSettings = keycloakOptions.Value;
    }

    [HttpGet("login-url")]
    public IActionResult GetLoginUrl()
    {
        if (_unleash.IsEnabled(UseKeycloakAuthFlag))
        {
            return Ok(new
            {
                provider = AuthSchemes.Keycloak,
                authority = _keycloakSettings.Authority,
                clientId = _keycloakSettings.ClientId,
                scopes = _keycloakSettings.Scopes
            });
        }

        return Ok(new
        {
            provider = AuthSchemes.Auth0,
            authority = $"https://{_auth0Settings.Domain}",
            clientId = _auth0Settings.ClientId,
            scopes = _auth0Settings.Scopes
        });
    }
}
