using AutoMapper;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.BLL.Services;

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken cancellationToken = default);
    Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken cancellationToken = default);
    Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task Remove(Guid id, CancellationToken cancellationToken = default);
    Task<ProductModel?> Update(UpdateProductModel model, CancellationToken cancellationToken = default);
}

public class ProductsService(IProductRepository repository, IMapper mapper) : IProductService
{
    public async Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken cancellationToken = default)
    {
        var pagedEntities = await repository.GetPaged(limit, continuationToken, cancellationToken);

        return mapper.Map<PagedResult<ProductModel>>(pagedEntities);
    }

    public async Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map<Product>(model);

        entity.SellerId = sellerId;

        var createdProduct = await repository.Add(entity, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create product.");

        return mapper.Map<ProductModel>(createdProduct);
    }

    public async Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: true, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return mapper.Map<ProductModel>(entity);
    }

    public async Task Remove(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetById(id, disableTracking: false, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        await repository.Delete(product, cancellationToken);
    }

    public async Task<ProductModel?> Update(UpdateProductModel model, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetById(model.Id, disableTracking: false, cancellationToken);

        if (product is null)
        {
            return null;
        }

        mapper.Map(model, product);

        await repository.Update(product, model.ImageUrls, cancellationToken);

        return mapper.Map<ProductModel>(product);
    }
}
