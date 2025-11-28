using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task<List<Product>> GetAll()
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Images)
            .ToListAsync();
    }
    public async Task<Product?> GetById(Guid id)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task Delete(Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }
    public async Task Update(Product product)
    {
        await context.SaveChangesAsync();

        await context.Entry(product).Reference(p => p.Category).LoadAsync();
    }
    public async Task<Product> Add(Product product)
    {
        var product1 = await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        await context.Entry(product).Reference(p => p.Category).LoadAsync();
        await context.Entry(product).Reference(p => p.Seller).LoadAsync();

        return product1.Entity;
    }
}
