using ProductService.BLL.DTO.Product;

namespace ProductService.BLL.Interfaces;

public interface IProductService
{
    Task<List<ProductViewModel>> GetAllAsync();
    Task<Guid> CreateAsync(CreateProductModel model, Guid sellerId);
}
