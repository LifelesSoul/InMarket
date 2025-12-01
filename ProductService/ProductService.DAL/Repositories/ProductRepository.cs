using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetPaged(int limit, Guid? lastId)
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

        return await query.Take(limit).ToListAsync();
    }

    public async Task<Product?> GetById(Guid id)
    {
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task Delete(Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }

    public async Task Update(Product product, IEnumerable<string>? newImageUrls)
    {
        context.Products.Update(product);

        if (newImageUrls != null)
        {
            var currentImages = await context.Set<ProductImage>()
                .Where(p => p.ProductId == product.Id)
                .ToListAsync();

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
        }

        await context.SaveChangesAsync();

        await context.Entry(product).Reference(p => p.Category).LoadAsync();
    }

    public async Task<Product> Add(Product product)
    {
        var newProduct = await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        await context.Entry(product).Reference(product => product.Category).LoadAsync();
        await context.Entry(product).Reference(product => product.Seller).LoadAsync();

        return newProduct.Entity;
    }
}
