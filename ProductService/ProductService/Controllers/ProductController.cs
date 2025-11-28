using Microsoft.AspNetCore.Mvc;
using ProductService.BLL.Interfaces;
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
    public async Task<ActionResult<List<ProductViewModel>>> GetAll()
    {
        var result = await _service.GetAll();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> GetById(Guid id)
    {
        var product = await _service.GetById(id);
        if (product == null)
            return NotFound($"Product with id {id} not found");

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model)
    {
        var createdProduct = await _service.Create(model, model.SellerId);

        return Ok(createdProduct);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Remove(id);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> Update(Guid id, [FromBody] UpdateProductModel model)
    {
        var updatedProduct = await _service.Update(id, model);

        if (updatedProduct == null)
            return NotFound($"Product with id {id} not found");

        return Ok(updatedProduct);
    }
}
