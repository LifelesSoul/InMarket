using AutoMapper;
using FluentValidation;
using ProductService.BLL.Models.Category;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.BLL.Services;

public class CategoryService(
    ICategoryRepository repository,
    IMapper mapper,
    IValidator<CreateCategoryModel> createValidator,
    IValidator<CategoryModel> updateValidator
    ) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryModel>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await repository.GetAll(cancellationToken);

        return mapper.Map<List<CategoryModel>>(entities);
    }

    public async Task<CategoryModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken, disableTracking: true)
            ?? throw new KeyNotFoundException($"Category with id {id} not found");

        return mapper.Map<CategoryModel>(entity);
    }

    public async Task<CategoryModel> Create(CreateCategoryModel model, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(model, cancellationToken);

        var entity = mapper.Map<Category>(model);

        var createdEntity = await repository.Add(entity, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create category.");

        return mapper.Map<CategoryModel>(createdEntity);
    }

    public async Task<CategoryModel> Update(CategoryModel model, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(model.Id, cancellationToken, disableTracking: false)
             ?? throw new KeyNotFoundException($"Category with id {model.Id} not found");

        await updateValidator.ValidateAndThrowAsync(model, cancellationToken);

        mapper.Map(model, entity);

        await repository.Update(entity, cancellationToken);

        return mapper.Map<CategoryModel>(entity);
    }

    public async Task Remove(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken, disableTracking: false)
             ?? throw new KeyNotFoundException($"Category with id {id} not found");

        await repository.Delete(entity, cancellationToken);
    }
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryModel>> GetAll(CancellationToken cancellationToken);
    Task<CategoryModel> GetById(Guid id, CancellationToken cancellationToken);
    Task<CategoryModel> Create(CreateCategoryModel model, CancellationToken cancellationToken);
    Task<CategoryModel> Update(CategoryModel model, CancellationToken cancellationToken);
    Task Remove(Guid id, CancellationToken cancellationToken);
}
