using ProductService.BLL.Interfaces;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Models.User;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.BLL.Services;

public class ProductsService(IProductRepository repository) : IProductService
{
    public async Task<List<ProductViewModel>> GetAll()
    {
        var entities = await repository.GetAll();
        var abc = repository.GetAll();

        return entities.Select(p => new ProductViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Price = p.Price,
            CategoryName = p.Category.Name,

            Seller = new SellerViewModel
            {
                Id = p.Seller.Id,
                Username = p.Seller.Username,
                Email = p.Seller.Email,
                RegistrationDate = p.Seller.RegistrationDate
            },

            CreatedAt = p.CreationDate,
            Description = p.Description,
            Priority = p.Priority,
            Status = p.Status,
            ImageUrl = p.Images.FirstOrDefault()?.Url
        }).ToList();
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
            CreationDate = DateTimeOffset.UtcNow,

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

        if (createdProduct == null)
            throw new Exception("Product creation failed");

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
        var p = await repository.GetById(id);

        if (p == null) return null;

        return new ProductViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Price = p.Price,
            CategoryName = p.Category.Name,
            Seller = new SellerViewModel
            {
                Id = p.Seller.Id,
                Username = p.Seller.Username,
                Email = p.Seller.Email,
                RegistrationDate = p.Seller.RegistrationDate
            },
            CreatedAt = p.CreationDate,
            Description = p.Description,
            Priority = p.Priority,
            Status = p.Status,
            ImageUrl = p.Images.FirstOrDefault()?.Url
        };
    }

    public async Task Remove(Guid id)
    {
        var product = await repository.GetById(id);
        if (product != null)
        {
            await repository.Delete(product);
        }
    }

    public async Task<ProductViewModel?> Update(Guid id, UpdateProductModel model)
    {
        var p = await repository.GetById(id);
        if (p == null) return null;

        p.Title = model.Title;
        p.Price = model.Price;
        p.Description = model.Description;
        p.CategoryId = model.CategoryId;

        if (model.ImageUrls != null)
        {
            p.Images.Clear();

            foreach (var url in model.ImageUrls)
            {
                p.Images.Add(new ProductImage
                {
                    Url = url
                });
            }
        }

        await repository.Update(p);

        return new ProductViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Price = p.Price,
            CategoryName = p.Category.Name,

            Seller = new SellerViewModel
            {
                Id = p.Seller.Id,
                Username = p.Seller.Username,
                Email = p.Seller.Email,
                RegistrationDate = p.Seller.RegistrationDate
            },

            CreatedAt = p.CreationDate,
            Description = p.Description,
            Priority = p.Priority,
            Status = p.Status,

            ImageUrl = p.Images.FirstOrDefault()?.Url
        };
    }
}
