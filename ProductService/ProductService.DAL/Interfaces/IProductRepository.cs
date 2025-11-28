using ProductService.Domain.Entities;

namespace ProductService.DAL.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAll();
    Task<Product> Add(Product product);
    Task<Product?> GetById(Guid id);
    Task Delete(Product product);
    Task Update(Product product);
}
