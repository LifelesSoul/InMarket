using AutoMapper;
using ProductService.BLL.Models.Category;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.BLL.Services;

public class CategoryService(ICategoryRepository repository, IMapper mapper) : ICategoryService
{
    public async Task<List<CategoryModel>> GetAll(CancellationToken cancellationToken = default)
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
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(model.Name));
        }

        var cleanName = model.Name.Trim();

        var isTaken = await repository.IsNameTaken(cleanName, excludeId: null, cancellationToken);
        if (isTaken)
        {
            throw new InvalidOperationException($"Category with name '{cleanName}' already exists.");
        }

        var entity = mapper.Map<Category>(model);

        entity.Name = cleanName;
        entity.Id = Guid.NewGuid();

        var createdEntity = await repository.Add(entity, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create category.");

        return mapper.Map<CategoryModel>(createdEntity);
    }

    public async Task<CategoryModel> Update(UpdateCategoryModel model, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(model.Id, disableTracking: false, cancellationToken)
             ?? throw new KeyNotFoundException($"Category with id {model.Id} not found");

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(model.Name));
        }

        var cleanName = model.Name.Trim();

        var isTaken = await repository.IsNameTaken(cleanName, excludeId: model.Id, cancellationToken);
        if (isTaken)
        {
            throw new InvalidOperationException($"Category with name '{cleanName}' already exists.");
        }

        entity.Name = cleanName;
        entity.Name = model.Name;

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
    Task<List<CategoryModel>> GetAll(CancellationToken cancellationToken = default);
    Task<CategoryModel> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryModel> Create(CreateCategoryModel model, CancellationToken cancellationToken = default);
    Task<CategoryModel> Update(UpdateCategoryModel model, CancellationToken cancellationToken = default);
    Task Remove(Guid id, CancellationToken cancellationToken = default);
}
