using Microsoft.AspNetCore.Mvc;
using ProductService.BLL.Interfaces;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductViewModel>>> GetAll(
    [FromQuery] int limit = 10,
    [FromQuery] Guid? continuationToken = null,
    CancellationToken ct = default
    )
    {
        var result = await _service.GetAll(limit, continuationToken, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> GetById(Guid id, CancellationToken ct = default)
    {
        var product = await _service.GetById(id, ct);
        if (product is null)
            return NotFound($"Product with id {id} not found");

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model, CancellationToken ct = default)
    {
        var createdProduct = await _service.Create(model, model.SellerId, ct);

        return Ok(createdProduct);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _service.Remove(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ProductViewModel>> Update([FromBody] UpdateProductModel model, CancellationToken ct = default)
    {
        var updatedProduct = await _service.Update(model, ct);

        if (updatedProduct is null)
            return NotFound($"Product with id {model.Id} not found");

        return Ok(updatedProduct);
    }
}
