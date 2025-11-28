using ProductService.BLL.Models.Product;

namespace ProductService.BLL.Interfaces;

public interface IProductService
{
    Task<List<ProductViewModel>> GetAll();
    Task<ProductViewModel> Create(CreateProductModel model, Guid sellerId);
    Task<ProductViewModel?> GetById(Guid id);
    Task Remove(Guid id);
    Task<ProductViewModel?> Update(Guid id, UpdateProductModel model);
}
