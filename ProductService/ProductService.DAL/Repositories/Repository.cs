using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;

namespace ProductService.DAL.Repositories;

public abstract class Repository<T>(ProductDbContext context) : IRepository<T> where T : class
{
    protected readonly ProductDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetById(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        if (disableTracking)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
        }

        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<T> Add(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public virtual async Task Update(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task Delete(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
}

public interface IRepository<T> where T : class
{
    Task<T?> GetById(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<T> Add(T entity, CancellationToken cancellationToken = default);
    Task Update(T entity, CancellationToken cancellationToken = default);
    Task Delete(T entity, CancellationToken cancellationToken = default);
}
