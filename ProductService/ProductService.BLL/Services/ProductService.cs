using AutoMapper;
using Hangfire;
using Microsoft.Extensions.Logging;
using ProductService.BLL.Constants;
using ProductService.BLL.Events;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;
using System.Transactions;

namespace ProductService.BLL.Services;

public class ProductsService(
    IProductRepository repository,
    IMapper mapper,
    ILogger<ProductsService> logger,
    IBackgroundJobClient backgroundJobClient) : IProductService
{
    public async Task<PagedResult<ProductModel>> GetAll(int limit, Guid? lastId, CancellationToken cancellationToken)
    {
        var pagedEntities = await repository.GetPaged(limit, lastId, cancellationToken);
        return mapper.Map<PagedResult<ProductModel>>(pagedEntities);
    }

    public async Task<ProductModel> Create(
    CreateProductModel model,
    Guid sellerId,
    string externalUserId,
    CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Product>(model);
        entity.SellerId = sellerId;

        Product createdProduct;

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            createdProduct = await repository.Add(entity, cancellationToken)
                             ?? throw new InvalidOperationException("Failed to create product.");

            await repository.SaveChangesAsync(cancellationToken);

            transaction.Complete();
        }

        try
        {
            var notificationEvent = mapper.Map<CreateNotificationEvent>(createdProduct, opt =>
            {
                opt.Items[nameof(CreateNotificationEvent.Title)] = NotificationMessages.ProductCreatedTitle;
                opt.Items[nameof(CreateNotificationEvent.Message)] = NotificationMessages.GetProductCreatedMessage(createdProduct.Title);
                opt.Items[nameof(CreateNotificationEvent.ExternalId)] = externalUserId;
            });

            backgroundJobClient.Enqueue<IEventPublisher>(publisher =>
                publisher.PublishNotification(notificationEvent));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Product {ProductId} created, but failed to enqueue notification event.", createdProduct.Id);
        }

        return mapper.Map<ProductModel>(createdProduct);
    }

    public async Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken, disableTracking: true)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        return mapper.Map<ProductModel>(entity);
    }

    public async Task Remove(Guid id, string externalUserId, CancellationToken cancellationToken)
    {
        var product = await repository.GetById(id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await repository.Delete(product, cancellationToken);

        var notificationEvent = mapper.Map<CreateNotificationEvent>(product, opt =>
        {
            opt.Items[nameof(CreateNotificationEvent.Title)] = NotificationMessages.ProductDeletedTitle;
            opt.Items[nameof(CreateNotificationEvent.Message)] = NotificationMessages.GetProductDeletedMessage(product.Title);
            opt.Items[nameof(CreateNotificationEvent.ExternalId)] = externalUserId;
        });

        await repository.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue<IEventPublisher>(publisher =>
            publisher.PublishNotification(notificationEvent));

        transaction.Complete();
    }

    public async Task<ProductModel?> Update(
        UpdateProductModel model,
        string externalUserId,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetById(model.Id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"Product {model.Id} not found");

        mapper.Map(model, product);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await repository.Update(product, model.ImageUrls, cancellationToken);

        var notificationEvent = mapper.Map<CreateNotificationEvent>(product, opt =>
        {
            opt.Items[nameof(CreateNotificationEvent.Title)] = NotificationMessages.ProductUpdatedTitle;
            opt.Items[nameof(CreateNotificationEvent.Message)] = NotificationMessages.GetProductUpdatedMessage(product.Title);
            opt.Items[nameof(CreateNotificationEvent.ExternalId)] = externalUserId;
        });

        await repository.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue<IEventPublisher>(publisher =>
            publisher.PublishNotification(notificationEvent));

        transaction.Complete();

        return mapper.Map<ProductModel>(product);
    }
}

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? lastId, CancellationToken cancellationToken);
    Task<ProductModel> Create(CreateProductModel model, Guid sellerId, string externalUserId, CancellationToken cancellationToken);
    Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken);
    Task Remove(Guid id, string externalUserId, CancellationToken cancellationToken);
    Task<ProductModel?> Update(UpdateProductModel model, string externalUserId, CancellationToken cancellationToken);
}
