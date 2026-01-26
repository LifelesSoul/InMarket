using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProductService.API.Configurations;
using ProductService.API.Models.Webhook;
using ProductService.BLL.Services;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/webhooks")]
[ExcludeFromCodeCoverage]
public class WebhooksController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly WebhookSettings _settings;

    public WebhooksController(IUserService userService, IOptions<WebhookSettings> options)
    {
        _userService = userService;
        _settings = options.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] Auth0UserDto dto, [FromHeader(Name = "X-API-KEY")] string apiKey)
    {
        if (apiKey != _settings.ApiKey)
        {
            return Unauthorized();
        }

        await _userService.SyncUserAsync(dto.ExternalId, dto.Email, CancellationToken.None);

        return Ok();
    }
}
