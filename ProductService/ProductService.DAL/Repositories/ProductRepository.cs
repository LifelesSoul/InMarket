using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetPaged(int limit, Guid? lastId, CancellationToken ct = default)
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

        return await query.Take(limit).ToListAsync(ct);
    }

    public async Task<Product?> GetById(Guid id, CancellationToken ct = default)
    {
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, ct);
    }

    public async Task Delete(Product product, CancellationToken ct = default)
    {
        context.Products.Remove(product);

        await context.SaveChangesAsync(ct);
    }

    public async Task Update(Product product, IEnumerable<string>? newImageUrls, CancellationToken ct = default)
    {
        context.Products.Update(product);

        if (newImageUrls != null)
        {
            var currentImages = await context.Set<ProductImage>()
                .Where(p => p.ProductId == product.Id)
                .ToListAsync(ct);

            var newUrlSet = newImageUrls.Distinct().ToHashSet();

            var imagesToDelete = currentImages
                .Where(img => !newUrlSet.Contains(img.Url))
                .ToList();

            if (imagesToDelete.Count > 0)
            {
                context.Set<ProductImage>().RemoveRange(imagesToDelete);
            }

            var existingUrlSet = currentImages.Select(i => i.Url).ToHashSet();

            var urlsToAdd = newUrlSet
                .Where(url => !existingUrlSet.Contains(url))
                .ToList();

            foreach (var url in urlsToAdd)
            {
                await context.Set<ProductImage>().AddAsync(new ProductImage
                {
                    ProductId = product.Id,
                    Url = url
                });
            }

            await context.Entry(product).Reference(p => p.Category).LoadAsync(ct);
            await context.Entry(product).Collection(p => p.Images).LoadAsync(ct);
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task<Product> Add(Product product, CancellationToken ct = default)
    {
        var newProduct = await context.Products.AddAsync(product, ct);

        await context.SaveChangesAsync(ct);

        await context.Entry(product).Reference(product => product.Category).LoadAsync(ct);
        await context.Entry(product).Reference(product => product.Seller).LoadAsync(ct);

        return newProduct.Entity;
    }
}
