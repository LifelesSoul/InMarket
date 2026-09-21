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
    private readonly IUserService _userService;

    public ProfileController(IProfileService profileService, IUserService userService)
    {
        _profileService = profileService;
        _userService = userService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var externalId = User.GetExternalId();

        var profile = await _profileService.GetProfileByExternalIdAsync(externalId);

        if (profile == null)
        {
            var email = User.GetEmail();

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email claim is missing from the token.");
            }

            await _userService.SyncUserAsync(externalId, email, cancellationToken);

            profile = await _profileService.GetProfileByExternalIdAsync(externalId);
        }

        if (profile == null)
        {
            return StatusCode(500, "Failed to create user profile.");
        }

        return Ok(profile);
    }
}
