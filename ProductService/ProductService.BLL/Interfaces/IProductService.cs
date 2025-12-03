using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;

namespace ProductService.BLL.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductViewModel>> GetAll(int limit, Guid? continuationToken, CancellationToken ct = default);
    Task<ProductViewModel> Create(CreateProductModel model, Guid sellerId, CancellationToken ct = default);
    Task<ProductViewModel?> GetById(Guid id, CancellationToken ct = default);
    Task Remove(Guid id, CancellationToken ct = default);
    Task<ProductViewModel?> Update(UpdateProductModel model, CancellationToken ct = default);
}
