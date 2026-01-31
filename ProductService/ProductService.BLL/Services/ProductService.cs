using AutoMapper;
using MassTransit;
using ProductService.BLL.Events;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.BLL.Services;

public class ProductsService(
    IProductRepository repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint) : IProductService
{
    public async Task<PagedResult<ProductModel>> GetAll(int limit, Guid? lastId, CancellationToken cancellationToken)
    {
        var pagedEntities = await repository.GetPaged(limit, lastId, cancellationToken);

        return mapper.Map<PagedResult<ProductModel>>(pagedEntities);
    }

    public async Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Product>(model);
        entity.SellerId = sellerId;

        var createdProduct = await repository.Add(entity, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create product.");

        var notificationEvent = mapper.Map<CreateNotificationEvent>(createdProduct, opt =>
        {
            opt.Items["Title"] = "Product created";
            opt.Items["Message"] = $"Your product '{createdProduct.Title}' has been successfully published!";
        });

        await publishEndpoint.Publish(notificationEvent, cancellationToken);

        return mapper.Map<ProductModel>(createdProduct);
    }

    public async Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken, disableTracking: true)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        return mapper.Map<ProductModel>(entity);
    }

    public async Task Remove(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetById(id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        await repository.Delete(product, cancellationToken);
    }

    public async Task<ProductModel?> Update(UpdateProductModel model, CancellationToken cancellationToken)
    {
        var product = await repository.GetById(model.Id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"Product {model.Id} not found");

        mapper.Map(model, product);

        await repository.Update(product, model.ImageUrls, cancellationToken);

        var notificationEvent = mapper.Map<CreateNotificationEvent>(product, opt =>
        {
            opt.Items["Title"] = "Product updated";
            opt.Items["Message"] = $"Your product '{product.Title}' has been successfully updated!";
        });

        await publishEndpoint.Publish(notificationEvent, cancellationToken);

        return mapper.Map<ProductModel>(product);
    }
}

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? lastId, CancellationToken cancellationToken);
    Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken cancellationToken);
    Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken);
    Task Remove(Guid id, CancellationToken cancellationToken);
    Task<ProductModel?> Update(UpdateProductModel model, CancellationToken cancellationToken);
}
