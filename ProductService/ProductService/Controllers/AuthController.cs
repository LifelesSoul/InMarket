using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProductService.API.Configurations;
using Unleash;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUnleash _unleash;
    private readonly Auth0Settings _auth0Settings;
    private readonly KeycloakSettings _keycloakSettings;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUnleash unleash,
        IOptions<Auth0Settings> auth0Options,
        IOptions<KeycloakSettings> keycloakOptions,
        IConfiguration configuration)
    {
        _unleash = unleash;
        _auth0Settings = auth0Options.Value;
        _keycloakSettings = keycloakOptions.Value;
        _configuration = configuration;
    }

    [HttpGet("login-url")]
    public IActionResult GetLoginUrl()
    {
        bool useKeycloak = _unleash.IsEnabled("use-keycloak-auth");

        string redirectUri = _configuration["Frontend:RedirectUri"] ?? "http://localhost:5173/";
        string responseType = "code";

        if (useKeycloak)
        {
            string keycloakUrl = $"{_keycloakSettings.Authority}/protocol/openid-connect/auth" +
                                 $"?client_id={_keycloakSettings.ClientId}" +
                                 $"&response_type={responseType}" +
                                 $"&redirect_uri={redirectUri}";

            return Ok(new { provider = "Keycloak", url = keycloakUrl });
        }

        string auth0Url = $"https://{_auth0Settings.Domain}/authorize" +
                          $"?client_id={_auth0Settings.ClientId}" +
                          $"&response_type={responseType}" +
                          $"&redirect_uri={redirectUri}";

        return Ok(new { provider = "Auth0", url = auth0Url });
    }
}
