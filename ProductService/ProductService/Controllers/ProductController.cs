using Microsoft.AspNetCore.Mvc;
using ProductService.API.ViewModels.User;
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
        var pagedModels = await service.GetAll(limit, continuationToken, cancellationToken);

        var viewModels = pagedModels.Items.Select(MapToViewModel).ToList();

        var result = new PagedResult<ProductViewModel>
        {
            Items = viewModels,
            ContinuationToken = pagedModels.ContinuationToken
        };

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await service.GetById(id, cancellationToken);

        if (model is null)
        {
            return NotFound($"Product with id {id} not found");
        }

        return Ok(MapToViewModel(model));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductModel model, CancellationToken cancellationToken = default)
    {
        var createdModel = await service.Create(model, model.SellerId, cancellationToken);

        return Ok(MapToViewModel(createdModel));
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
        var updatedModel = await service.Update(model, cancellationToken);

        if (updatedModel is null)
        {
            return NotFound($"Product with id {model.Id} not found");
        }

        return Ok(MapToViewModel(updatedModel));
    }

    private static ProductViewModel MapToViewModel(ProductModel model)
    {
        return new ProductViewModel
        {
            Id = model.Id,
            Title = model.Title,
            Price = model.Price,
            CategoryName = model.Category.Name,
            Description = model.Description,
            Priority = model.Priority,
            Status = model.Status,
            ImageUrl = model.ImageUrls.FirstOrDefault(),
            CreatedAt = model.CreatedAt,
            Seller = new SellerViewModel
            {
                Id = model.Seller.Id,
                Username = model.Seller.Username,
                Email = model.Seller.Email,
            }
        };
    }
}