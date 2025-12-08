using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Services;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var model = await service.GetById(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        return Ok(mapper.Map<ProductViewModel>(model));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model, CancellationToken cancellationToken = default)
    {
        var createdModel = await service.Create(model, model.SellerId, cancellationToken);

        return Ok(mapper.Map<ProductViewModel>(createdModel));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await service.Remove(id, cancellationToken);

        return NoContent();
    }

    [HttpPut]
    public async Task<ActionResult<ProductViewModel>> Update([FromBody] UpdateProductModel model, CancellationToken cancellationToken = default)
    {
        var updatedModel = await service.Update(model, cancellationToken);

        return Ok(mapper.Map<ProductViewModel>(updatedModel));
    }
}
