using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.DAL.Repositories;

[ExcludeFromCodeCoverage]
public abstract class Repository<T>(ProductDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly ProductDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetById(Guid id, CancellationToken cancellationToken, bool disableTracking = false)
    {
        if (disableTracking)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        return await DbSet.FindAsync(id, cancellationToken);
    }

    public virtual async Task<T> Add(T entity, CancellationToken cancellationToken)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task Update(T entity, CancellationToken cancellationToken)
    {
        DbSet.Update(entity);
    }

    public virtual async Task Delete(T entity, CancellationToken cancellationToken)
    {
        DbSet.Remove(entity);
    }

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await Context.SaveChangesAsync(cancellationToken);
    }
}

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetById(Guid id, CancellationToken cancellationToken, bool disableTracking = false);
    Task<T> Add(T entity, CancellationToken cancellationToken);
    Task Update(T entity, CancellationToken cancellationToken);
    Task Delete(T entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
