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

        if (newImageUrls is not null)
        {
            var incomingUrls = newImageUrls.ToList();

            var currentImages = await context.Set<ProductImage>()
                .Where(p => p.ProductId == product.Id)
                .ToListAsync(ct);

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

                await context.Set<ProductImage>().AddRangeAsync(imagesToAdd, ct);
            }
        }

        await context.SaveChangesAsync(ct);

        await context.Entry(product).Reference(p => p.Category).LoadAsync(ct);
        await context.Entry(product).Collection(p => p.Images).LoadAsync(ct);
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
