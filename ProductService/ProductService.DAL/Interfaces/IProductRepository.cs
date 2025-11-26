using ProductService.Domain.Entities;

namespace ProductService.DAL.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllWithDetailsAsync();
    Task AddAsync(Product product);
}
