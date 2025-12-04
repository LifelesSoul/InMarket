using Microsoft.AspNetCore.Mvc;
using ProductService.BLL.Interfaces;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductViewModel>>> GetAll(
        [FromQuery] int limit = 10,
        [FromQuery] Guid? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAll(limit, continuationToken, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await service.GetById(id, cancellationToken);

        if (product is null)
        {
            return NotFound($"Product with id {id} not found");
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model, CancellationToken cancellationToken = default)
    {
        var createdProduct = await service.Create(model, model.SellerId, cancellationToken);

        return Ok(createdProduct);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await service.Remove(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ProductViewModel>> Update([FromBody] UpdateProductModel model, CancellationToken cancellationToken = default)
    {
        var updatedProduct = await service.Update(model, cancellationToken);

        if (updatedProduct is null)
        {
            return NotFound($"Product with id {model.Id} not found");
        }

        return Ok(updatedProduct);
    }
}