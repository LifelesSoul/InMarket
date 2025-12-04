using ProductService.Domain.Entities;

namespace ProductService.DAL.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetPaged(int limit, Guid? lastId, CancellationToken ct = default);
    Task<Product?> GetById(Guid id, bool disableTraсking = false, CancellationToken ct = default);
    Task Delete(Product product, CancellationToken ct = default);
    Task<Product> Add(Product product, CancellationToken ct = default);
    Task Update(Product product, IEnumerable<string>? newImageUrls, CancellationToken ct = default);
}
