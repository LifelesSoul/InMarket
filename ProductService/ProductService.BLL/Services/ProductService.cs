using ProductService.BLL.Interfaces;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Models.User;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.BLL.Services;

public class ProductsService(IProductRepository repository) : IProductService
{
    public async Task<PagedResult<ProductViewModel>> GetAll(int limit, string? continuationToken)
    {
        Guid? lastId = null;
        if (Guid.TryParse(continuationToken, out var parsedId))
        {
            lastId = parsedId;
        }

        var entities = await repository.GetPaged(limit, lastId);

        var viewModels = entities.Select(productView => new ProductViewModel
        {
            Id = productView.Id,
            Title = productView.Title,
            Price = productView.Price,
            CategoryName = productView.Category.Name,
            Seller = new SellerViewModel
            {
                Id = productView.Seller.Id,
                Username = productView.Seller.Username,
                Email = productView.Seller.Email,
                RegistrationDate = productView.Seller.RegistrationDate
            },
            CreatedAt = productView.CreationDate,
            Description = productView.Description,
            Priority = productView.Priority,
            Status = productView.Status,
            ImageUrl = productView.Images.FirstOrDefault()?.Url
        }).ToList();

        string? nextToken = null;
        if (viewModels.Count > 0)
        {
            nextToken = viewModels.Last().Id.ToString();
        }

        return new PagedResult<ProductViewModel>
        {
            Items = viewModels,
            ContinuationToken = nextToken
        };
    }

    public async Task<ProductViewModel> Create(CreateProductModel model, Guid sellerId)
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

        var createdProduct = await repository.Add(entity);

        if (createdProduct is null)
            throw new InvalidOperationException("Failed to create product. The database did not return the entity.");

        return new ProductViewModel
        {
            Id = createdProduct.Id,
            Title = createdProduct.Title,
            Price = createdProduct.Price,
            Description = createdProduct.Description,

            CategoryName = createdProduct.Category.Name,

            Seller = new SellerViewModel
            {
                Id = createdProduct.Seller.Id,
                Username = createdProduct.Seller.Username,
                Email = createdProduct.Seller.Email,
                RegistrationDate = createdProduct.Seller.RegistrationDate
            },

            Priority = createdProduct.Priority,
            Status = createdProduct.Status,
            CreatedAt = createdProduct.CreationDate,
            ImageUrl = createdProduct.Images.FirstOrDefault()?.Url
        };
    }

    public async Task<ProductViewModel?> GetById(Guid id)
    {
        var productView = await repository.GetById(id);

        if (productView is null) return null;

        return new ProductViewModel
        {
            Id = productView.Id,
            Title = productView.Title,
            Price = productView.Price,
            CategoryName = productView.Category.Name,
            Seller = new SellerViewModel
            {
                Id = productView.Seller.Id,
                Username = productView.Seller.Username,
                Email = productView.Seller.Email,
                RegistrationDate = productView.Seller.RegistrationDate
            },
            CreatedAt = productView.CreationDate,
            Description = productView.Description,
            Priority = productView.Priority,
            Status = productView.Status,
            ImageUrl = productView.Images.FirstOrDefault()?.Url
        };
    }

    public async Task<bool> Remove(Guid id)
    {
        var product = await repository.GetById(id);
        if (product == null)
        {
            return false;
        }

        await repository.Delete(product);
        return true;
    }

    public async Task<ProductViewModel?> Update(Guid id, UpdateProductModel model)
    {
        var productView = await repository.GetById(id);
        if (productView is null) return null;

        productView.Title = model.Title;
        productView.Price = model.Price;
        productView.Description = model.Description;
        productView.CategoryId = model.CategoryId;
        productView.Status = model.Status;

        await repository.Update(productView, model.ImageUrls);

        var updatedProduct = await repository.GetById(id);

        return new ProductViewModel
        {
            Id = updatedProduct.Id,
            Title = updatedProduct.Title,
            Price = updatedProduct.Price,
            CategoryName = updatedProduct.Category?.Name,
            Seller = new SellerViewModel
            {
                Id = updatedProduct.Seller.Id,
                Username = updatedProduct.Seller.Username,
                Email = updatedProduct.Seller.Email,
                RegistrationDate = updatedProduct.Seller.RegistrationDate
            },
            CreatedAt = updatedProduct.CreationDate,
            Description = updatedProduct.Description,
            Priority = updatedProduct.Priority,
            Status = updatedProduct.Status,
            ImageUrl = updatedProduct.Images.FirstOrDefault()?.Url
        };
    }
}
