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
    public async Task<IReadOnlyList<CategoryModel>> GetAll(CancellationToken cancellationToken = default)
    {
        var entities = await repository.GetAll(cancellationToken);

        return mapper.Map<List<CategoryModel>>(entities);
    }

    public async Task<CategoryModel> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: true, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {id} not found");

        return mapper.Map<CategoryModel>(entity);
    }

    public async Task<CategoryModel> Create(CreateCategoryModel model, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(model, cancellationToken);

        var entity = mapper.Map<Category>(model);

        var createdEntity = await repository.Add(entity, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create category.");

        return mapper.Map<CategoryModel>(createdEntity);
    }

    public async Task<CategoryModel> Update(CategoryModel model, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(model.Id, disableTracking: false, cancellationToken)
             ?? throw new KeyNotFoundException($"Category with id {model.Id} not found");

        await updateValidator.ValidateAndThrowAsync(model, cancellationToken);

        await repository.Update(entity, cancellationToken);

        return mapper.Map<CategoryModel>(entity);
    }

    public async Task Remove(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: false, cancellationToken)
             ?? throw new KeyNotFoundException($"Category with id {id} not found");

        await repository.Delete(entity, cancellationToken);
    }
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryModel>> GetAll(CancellationToken cancellationToken = default);
    Task<CategoryModel> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryModel> Create(CreateCategoryModel model, CancellationToken cancellationToken = default);
    Task<CategoryModel> Update(CategoryModel model, CancellationToken cancellationToken = default);
    Task Remove(Guid id, CancellationToken cancellationToken = default);
}
