using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;
public interface ICategoryRepository
{
    Task<List<Category>> GetAll(CancellationToken cancellationToken = default);
    Task<Category?> GetById(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<Category> Add(Category category, CancellationToken cancellationToken = default);
    Task Update(Category category, CancellationToken cancellationToken = default);
    Task Delete(Category category, CancellationToken cancellationToken = default);
}
public class CategoryRepository(ProductDbContext context) : ICategoryRepository
{
    public async Task<List<Category>> GetAll(CancellationToken cancellationToken = default)
    {
        return await context.Set<Category>()
            .AsNoTracking()
            .OrderBy(context => context.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetById(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = context.Set<Category>().AsQueryable();

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(context => context.Id == id, cancellationToken);
    }

    public async Task<Category> Add(Category category, CancellationToken cancellationToken = default)
    {
        var result = await context.Set<Category>().AddAsync(category, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return result.Entity;
    }

    public async Task Update(Category category, CancellationToken cancellationToken = default)
    {
        context.Set<Category>().Update(category);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(Category category, CancellationToken cancellationToken = default)
    {
        context.Set<Category>().Remove(category);

        await context.SaveChangesAsync(cancellationToken);
    }
}
