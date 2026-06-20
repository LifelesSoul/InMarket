using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Extensions;
using ProductService.BLL.Services;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var externalId = User.GetExternalId();

        var profile = await _profileService.GetProfileByExternalIdAsync(externalId);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}
