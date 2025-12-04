using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Repositories;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.BLL.Services;

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken ct = default);
    Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken ct = default);
    Task<ProductModel?> GetById(Guid id, CancellationToken ct = default);
    Task Remove(Guid id, CancellationToken ct = default);
    Task<ProductModel?> Update(UpdateProductModel model, CancellationToken ct = default);
}

public class ProductsService(IProductRepository repository) : IProductService
{
    public async Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken ct = default)
    {
        var entities = await repository.GetPaged(limit, continuationToken, ct);

        var models = entities.Select(e => new ProductModel
        {
            Id = e.Id,
            Title = e.Title,
            Price = e.Price,
            Description = e.Description,
            CreatedAt = e.CreationDate,
            Priority = e.Priority,
            Status = e.Status,

            Category = new CategoryModel
            {
                Id = e.Category.Id,
                Name = e.Category.Name
            },
            Seller = new SellerModel
            {
                Id = e.Seller.Id,
                Username = e.Seller.Username,
                Email = e.Seller.Email
            },

            ImageUrls = e.Images.Select(i => i.Url).ToList()
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

    public async Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken ct = default)
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

        var createdProduct = await repository.Add(entity, ct)
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

    public async Task<ProductModel?> GetById(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetById(id, disableTracking: true, ct);

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

    public async Task Remove(Guid id, CancellationToken ct = default)
    {
        var product = await repository.GetById(id, disableTracking: false, ct)
            ?? throw new KeyNotFoundException($"Product {id} not found");

        await repository.Delete(product, ct);
    }

    public async Task<ProductModel?> Update(UpdateProductModel model, CancellationToken ct = default)
    {
        var product = await repository.GetById(model.Id, disableTracking: false, ct);

        if (product is null)
        {
            return null;
        }

        product.Title = model.Title;
        product.Price = model.Price;
        product.Description = model.Description;
        product.CategoryId = model.CategoryId;
        product.Status = model.Status;

        await repository.Update(product, model.ImageUrls, ct);

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
