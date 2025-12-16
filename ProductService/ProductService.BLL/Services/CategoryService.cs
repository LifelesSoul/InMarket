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

        var entity = mapper.Map<Category>(model);

        entity.Name = NormalizeName(model.Name);

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

        var cleanName = NormalizeName(model.Name);

        entity.Name = cleanName;

        await repository.Update(entity, cancellationToken);

        return mapper.Map<CategoryModel>(entity);
    }

    public async Task Remove(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: false, cancellationToken)
             ?? throw new KeyNotFoundException($"Category with id {id} not found");

        await repository.Delete(entity, cancellationToken);
    }

    private static string NormalizeName(string inputName)
    {
        if (string.IsNullOrWhiteSpace(inputName))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(inputName));
        }

        var trimmed = inputName.Trim();

        var lowerCased = trimmed.ToLower();

        return char.ToUpper(lowerCased[0]) + lowerCased[1..];
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
