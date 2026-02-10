using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Extensions;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Services;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class ProductsController(IProductService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductViewModel>>> GetAll(
        [FromQuery] int limit = 10,
        [FromQuery] Guid? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var pagedModels = await service.GetAll(limit, continuationToken, cancellationToken);

        var result = mapper.Map<PagedResult<ProductViewModel>>(pagedModels);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await service.GetById(id, cancellationToken);

        return Ok(mapper.Map<ProductViewModel>(model));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model, CancellationToken cancellationToken = default)
    {
        var externalUserId = User.GetExternalId();

        var createdModel = await service.Create(model, model.SellerId, externalUserId, cancellationToken);

        return Ok(mapper.Map<ProductViewModel>(createdModel));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var externalUserId = User.GetExternalId();

        await service.Remove(id, externalUserId, cancellationToken);

        return NoContent();
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ProductViewModel>> Update([FromBody] UpdateProductModel model, CancellationToken cancellationToken = default)
    {
        var externalUserId = User.GetExternalId();

        var updatedModel = await service.Update(model, externalUserId, cancellationToken);

        return Ok(mapper.Map<ProductViewModel>(updatedModel));
    }
}
