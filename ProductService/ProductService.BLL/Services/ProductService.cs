using ProductService.BLL.DTO.Product;
using ProductService.BLL.Interfaces;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.BLL.Services;

public class ProductsService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductsService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductViewModel>> GetAllAsync()
    {
        var entities = await _repository.GetAllWithDetailsAsync();

        var viewModels = entities.Select(productView => new ProductViewModel
        {
            Id = productView.Id,
            Title = productView.Title,
            PriceString = $"{productView.Price} rub.",
            CategoryName = productView.Category.Name,
            SellerName = productView.Seller.Username,
            CreatedAt = productView.CreationDate
        }).ToList();

        return viewModels;
    }

    public async Task<Guid> CreateAsync(CreateProductModel model, Guid sellerId)
    {
        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Price = model.Price,
            Priority = Priority.Low,
            Status = ProductStatus.Available,
            CreationDate = DateTimeOffset.UtcNow,
            CategoryId = model.CategoryId,
            SellerId = sellerId,

            Category = null!,
            Seller = null!
        };

        await _repository.AddAsync(entity);

        return entity.Id;
    }
}
