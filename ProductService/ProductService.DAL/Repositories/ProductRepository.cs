using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllWithDetailsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }
}
