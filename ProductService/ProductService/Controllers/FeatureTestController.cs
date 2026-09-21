using Microsoft.AspNetCore.Mvc;
using Unleash;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/test-feature")]
[ExcludeFromCodeCoverage]
public class FeatureTestController : ControllerBase
{
    private readonly IUnleash _unleash;

    public FeatureTestController(IUnleash unleash)
    {
        _unleash = unleash;
    }

    [HttpGet]
    public IActionResult CheckFeature()
    {
        bool isFeatureEnabled = _unleash.IsEnabled("show-new-product-feature");

        if (isFeatureEnabled)
        {
            return Ok(new { message = "The new feature is ENABLED! 🎉", status = true });
        }

        return Ok(new { message = "The new feature is still disabled. Running the old path 🧱", status = false });
    }
}
