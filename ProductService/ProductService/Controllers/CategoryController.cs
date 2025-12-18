using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Models;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Services;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryViewModel>>> GetAll(CancellationToken cancellationToken)
    {
        var models = await service.GetAll(cancellationToken);

        var viewModels = mapper.Map<List<CategoryViewModel>>(models);

        return Ok(viewModels);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryViewModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await service.GetById(id, cancellationToken);

        return Ok(mapper.Map<CategoryViewModel>(model));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryViewModel>> Create([FromBody] CreateCategoryModel createModel, CancellationToken cancellationToken)
    {
        var createdModel = await service.Create(createModel, cancellationToken);

        return Ok(mapper.Map<CategoryViewModel>(createdModel));
    }

    [HttpPut]
    public async Task<ActionResult<CategoryViewModel>> Update([FromBody] CategoryModel updateModel, CancellationToken cancellationToken)
    {
        var updatedModel = await service.Update(updateModel, cancellationToken);

        return Ok(mapper.Map<CategoryViewModel>(updatedModel));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Remove(id, cancellationToken);

        return NoContent();
    }
}
