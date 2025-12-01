using ProductService.Domain.Entities;

namespace ProductService.DAL.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetPaged(int limit, Guid? lastId);
    Task Update(Product product, IEnumerable<string>? newImageUrls);
    Task<Product> Add(Product product);
    Task<Product?> GetById(Guid id);
    Task Delete(Product product);
    //Task Update(Product product);
}
