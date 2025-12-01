using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;

namespace ProductService.BLL.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductViewModel>> GetAll(int limit, string? continuationToken);
    Task<ProductViewModel> Create(CreateProductModel model, Guid sellerId);
    Task<ProductViewModel?> GetById(Guid id);
    Task<bool> Remove(Guid id);
    Task<ProductViewModel?> Update(Guid id, UpdateProductModel model);
}
