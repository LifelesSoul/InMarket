using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public class CategoryRepository(ProductDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAll(CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(context => context.Name)
            .ToListAsync(cancellationToken);
    }
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAll(CancellationToken cancellationToken);
}
