using Microsoft.AspNetCore.Mvc;
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
    private readonly IConfiguration _configuration;

    public WebhooksController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] Auth0UserDto dto, [FromHeader(Name = "X-API-KEY")] string apiKey)
    {
        var secretKey = _configuration["Webhooks:ApiKey"];

        if (apiKey != secretKey)
        {
            return Unauthorized();
        }

        await _userService.SyncUserAsync(dto.ExternalId, dto.Email, CancellationToken.None);

        return Ok();
    }
}
