using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;

namespace ProductService.BLL.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductModel>> GetAll(int limit, Guid? continuationToken, CancellationToken ct = default);
    Task<ProductModel> Create(CreateProductModel model, Guid sellerId, CancellationToken ct = default);
    Task<ProductModel?> GetById(Guid id, CancellationToken ct = default);
    Task Remove(Guid id, CancellationToken ct = default);
    Task<ProductModel?> Update(UpdateProductModel model, CancellationToken ct = default);
}
