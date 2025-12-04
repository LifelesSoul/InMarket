using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetPaged(int limit, Guid? lastId, CancellationToken ct = default);
    Task<Product?> GetById(Guid id, bool disableTracking = false, CancellationToken ct = default);
    Task Delete(Product product, CancellationToken ct = default);
    Task<Product> Add(Product product, CancellationToken ct = default);
    Task Update(Product product, IEnumerable<string>? newImageUrls, CancellationToken ct = default);
}

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetPaged(int limit, Guid? lastId, CancellationToken cancellationToken = default)
    {
        var query = context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Seller)
            .Include(product => product.Images)
            .OrderBy(product => product.Id);

        if (lastId.HasValue)
        {
            query = (IOrderedQueryable<Product>)query.Where(product => product.Id > lastId.Value);
        }

        return await query.Take(limit).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetById(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = context.Products.AsQueryable();

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task Delete(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Remove(product);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(Product product, IEnumerable<string>? newImageUrls, CancellationToken cancellationToken = default)
    {
        if (newImageUrls is not null)
        {
            var incomingUrls = newImageUrls.ToList();

            var currentImages = await context.Set<ProductImage>()
                .Where(p => p.ProductId == product.Id)
                .ToListAsync(cancellationToken);

            var imagesToDelete = new List<ProductImage>();

            foreach (var dbImage in currentImages)
            {
                var index = incomingUrls.IndexOf(dbImage.Url);

                if (index is not -1)
                {
                    incomingUrls.RemoveAt(index);
                }
                else
                {
                    imagesToDelete.Add(dbImage);
                }
            }

            if (imagesToDelete.Count > 0)
            {
                context.Set<ProductImage>().RemoveRange(imagesToDelete);
            }

            if (incomingUrls.Count > 0)
            {
                var imagesToAdd = incomingUrls.Select(url => new ProductImage
                {
                    ProductId = product.Id,
                    Url = url
                }).ToList();

                await context.Set<ProductImage>().AddRangeAsync(imagesToAdd, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product> Add(Product product, CancellationToken cancellationToken = default)
    {
        var newProduct = await context.Products.AddAsync(product, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(product).Reference(product => product.Category).LoadAsync(cancellationToken);
        await context.Entry(product).Reference(product => product.Seller).LoadAsync(cancellationToken);

        return newProduct.Entity;
    }
}
