using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Models;
using ProductService.BLL.Models.User;
using ProductService.BLL.Services;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserViewModel>>> GetAll(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var models = await service.GetAll(page, pageSize, cancellationToken);

        var viewModels = mapper.Map<List<UserViewModel>>(models);

        return Ok(viewModels);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserViewModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await service.GetById(id, cancellationToken);

        return Ok(mapper.Map<UserViewModel>(model));
    }

    [HttpPost]
    public async Task<ActionResult<UserViewModel>> Create([FromBody] CreateUserModel createModel, CancellationToken cancellationToken)
    {
        var createdModel = await service.Create(createModel, cancellationToken);

        return Ok(mapper.Map<UserViewModel>(createdModel));
    }

    [HttpPut]
    public async Task<ActionResult<UserViewModel>> Update([FromBody] UpdateUserModel updateModel, CancellationToken cancellationToken)
    {
        var updatedModel = await service.Update(updateModel, cancellationToken);

        return Ok(mapper.Map<UserViewModel>(updatedModel));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);

        return NoContent();
    }
}
