using Microsoft.AspNetCore.Mvc;
using ProductService.BLL.DTO.Product;
using ProductService.BLL.Interfaces;

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
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model)
    {

        var id = await _service.CreateAsync(model, model.SellerId);
        return Ok(id);
    }
}
