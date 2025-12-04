using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.BLL.Services;

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken cancellationToken = default);
    Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken cancellationToken = default);
    Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task Remove(Guid id, CancellationToken cancellationToken = default);
    Task<ProductModel?> Update(UpdateProductModel model, CancellationToken cancellationToken = default);
}

public class ProductsService(IProductRepository repository) : IProductService
{
    public async Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken cancellationToken = default)
    {
        var entities = await repository.GetPaged(limit, continuationToken, cancellationToken);

        var models = entities.Select(entity => new ProductModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Price = entity.Price,
            Description = entity.Description,
            CreatedAt = entity.CreationDate,
            Priority = entity.Priority,
            Status = entity.Status,

            Category = new CategoryModel
            {
                Id = entity.Category.Id,
                Name = entity.Category.Name
            },
            Seller = new SellerModel
            {
                Id = entity.Seller.Id,
                Username = entity.Seller.Username,
                Email = entity.Seller.Email
            },

            ImageUrls = entity.Images.Select(i => i.Url).ToList()
        }).ToList();

        string? nextToken = null;
        if (models.Count > 0)
        {
            nextToken = models.Last().Id.ToString();
        }

        return new PagedResult<ProductModel>
        {
            Items = models,
            ContinuationToken = nextToken
        };
    }

    public async Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken cancellationToken = default)
    {
        var entity = new Product
        {
            Title = model.Title,
            Price = model.Price,
            Description = model.Description,
            Priority = Priority.Low,
            Status = ProductStatus.Available,
            CreationDate = TimeProvider.System.GetUtcNow(),
            CategoryId = model.CategoryId,
            SellerId = sellerId,
            Images = model.ImageUrls?.Select(url => new ProductImage
            {
                Url = url
            }).ToList() ?? new List<ProductImage>(),
            Category = null!,
            Seller = null!
        };

        var createdProduct = await repository.Add(entity, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create product.");

        return new ProductModel
        {
            Id = createdProduct.Id,
            Title = createdProduct.Title,
            Price = createdProduct.Price,
            Description = createdProduct.Description,
            CreatedAt = createdProduct.CreationDate,
            Priority = createdProduct.Priority,
            Status = createdProduct.Status,
            Category = new CategoryModel
            {
                Id = createdProduct.Category.Id,
                Name = createdProduct.Category.Name
            },
            Seller = new SellerModel
            {
                Id = createdProduct.Seller.Id,
                Username = createdProduct.Seller.Username,
                Email = createdProduct.Seller.Email
            },
            ImageUrls = createdProduct.Images.Select(i => i.Url).ToList()
        };
    }

    public async Task<ProductModel?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: true, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new ProductModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Price = entity.Price,
            Description = entity.Description,
            CreatedAt = entity.CreationDate,
            Priority = entity.Priority,
            Status = entity.Status,
            Category = new CategoryModel
            {
                Id = entity.Category.Id,
                Name = entity.Category.Name
            },
            Seller = new SellerModel
            {
                Id = entity.Seller.Id,
                Username = entity.Seller.Username,
                Email = entity.Seller.Email
            },
            ImageUrls = entity.Images.Select(i => i.Url).ToList()
        };
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

        product.Title = model.Title;
        product.Price = model.Price;
        product.Description = model.Description;
        product.CategoryId = model.CategoryId;
        product.Status = model.Status;

        await repository.Update(product, model.ImageUrls, cancellationToken);

        return new ProductModel
        {
            Id = product.Id,
            Title = product.Title,
            Price = product.Price,
            Description = product.Description,
            CreatedAt = product.CreationDate,
            Priority = product.Priority,
            Status = product.Status,
            Category = new CategoryModel
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            },
            Seller = new SellerModel
            {
                Id = product.Seller.Id,
                Username = product.Seller.Username,
                Email = product.Seller.Email
            },
            ImageUrls = product.Images.Select(i => i.Url).ToList()
        };
    }
}
