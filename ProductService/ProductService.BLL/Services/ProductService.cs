using AutoMapper;
using FluentValidation;
using Hangfire;
using Microsoft.Extensions.Logging;
using ProductService.BLL.Constants;
using ProductService.BLL.Events;
using ProductService.BLL.Exceptions;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Models;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using System.Transactions;
using UserService.Domain.Enums;

namespace ProductService.BLL.Services;

public class ProductsService(
    IProductRepository repository,
    IUserRepository userRepository,
    IMapper mapper,
    ILogger<ProductsService> logger,
    IBackgroundJobClient backgroundJobClient,
    IValidator<CreateProductModel> createValidator,
    IValidator<UpdateProductModel> updateValidator) : IProductService
{
    public async Task<PagedResult<ProductModel>> GetAll(int limit, Guid? lastId, CancellationToken cancellationToken)
    {
        var filter = new ProductFilter(Status: ProductStatus.Available);

        var pagedEntities = await repository.GetPaged(limit, lastId, filter, cancellationToken);
        return mapper.Map<PagedResult<ProductModel>>(pagedEntities);
    }

    public async Task<PagedResult<ProductModel>> GetMine(
        Caller caller,
        int limit,
        Guid? lastId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalId(caller.ExternalId, cancellationToken);

        if (user is null)
        {
            return new PagedResult<ProductModel>();
        }

        var filter = new ProductFilter(SellerId: user.Id);

        var pagedEntities = await repository.GetPaged(limit, lastId, filter, cancellationToken);
        return mapper.Map<PagedResult<ProductModel>>(pagedEntities);
    }

    public async Task<ProductModel> Create(
        CreateProductModel model,
        Caller caller,
        CancellationToken cancellationToken)
    {
        var seller = await userRepository.GetByExternalId(caller.ExternalId, cancellationToken)
            ?? throw new ForbiddenException("Open your profile once before publishing products.");

        if (!seller.Role.HasFlag(UserRoles.Seller))
        {
            throw new ForbiddenException("Only sellers can publish products.");
        }

        await createValidator.ValidateAndThrowAsync(model, cancellationToken);

        var entity = mapper.Map<Product>(model);
        entity.SellerId = seller.Id;

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
                opt.Items[nameof(CreateNotificationEvent.ExternalId)] = caller.ExternalId;
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

    public async Task Remove(Guid id, Caller caller, CancellationToken cancellationToken)
    {
        var product = await repository.GetById(id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        await EnsureCanManage(product, caller, cancellationToken);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await repository.Delete(product, cancellationToken);

        var notificationEvent = mapper.Map<CreateNotificationEvent>(product, opt =>
        {
            opt.Items[nameof(CreateNotificationEvent.Title)] = NotificationMessages.ProductDeletedTitle;
            opt.Items[nameof(CreateNotificationEvent.Message)] = NotificationMessages.GetProductDeletedMessage(product.Title);
            opt.Items[nameof(CreateNotificationEvent.ExternalId)] = caller.ExternalId;
        });

        await repository.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue<IEventPublisher>(publisher =>
            publisher.PublishNotification(notificationEvent));

        transaction.Complete();
    }

    public async Task<ProductModel?> Update(
        UpdateProductModel model,
        Caller caller,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetById(model.Id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"Product {model.Id} not found");

        await EnsureCanManage(product, caller, cancellationToken);

        await updateValidator.ValidateAndThrowAsync(model, cancellationToken);

        mapper.Map(model, product);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await repository.Update(product, model.ImageUrls, cancellationToken);

        var notificationEvent = mapper.Map<CreateNotificationEvent>(product, opt =>
        {
            opt.Items[nameof(CreateNotificationEvent.Title)] = NotificationMessages.ProductUpdatedTitle;
            opt.Items[nameof(CreateNotificationEvent.Message)] = NotificationMessages.GetProductUpdatedMessage(product.Title);
            opt.Items[nameof(CreateNotificationEvent.ExternalId)] = caller.ExternalId;
        });

        await repository.SaveChangesAsync(cancellationToken);

        backgroundJobClient.Enqueue<IEventPublisher>(publisher =>
            publisher.PublishNotification(notificationEvent));

        transaction.Complete();

        return mapper.Map<ProductModel>(product);
    }

    private async Task EnsureCanManage(Product product, Caller caller, CancellationToken cancellationToken)
    {
        if (caller.IsAdmin)
        {
            return;
        }

        var user = await userRepository.GetByExternalId(caller.ExternalId, cancellationToken);

        if (user is null || user.Id != product.SellerId)
        {
            throw new ForbiddenException("Only the owner or an admin can change this product.");
        }
    }
}

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? lastId, CancellationToken cancellationToken);
    Task<PagedResult<ProductModel>> GetMine(Caller caller, int limit, Guid? lastId, CancellationToken cancellationToken);
    Task<ProductModel> Create(CreateProductModel model, Caller caller, CancellationToken cancellationToken);
    Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken);
    Task Remove(Guid id, Caller caller, CancellationToken cancellationToken);
    Task<ProductModel?> Update(UpdateProductModel model, Caller caller, CancellationToken cancellationToken);
}
