using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using UserService.Domain.Entities;

namespace ProductService.DAL.Repositories;

public class UserRepository(ProductDbContext context) : Repository<User>(context), IUserRepository
{
    public override async Task<User?> GetById(Guid id, CancellationToken cancellationToken, bool disableTracking = false)
    {
        IQueryable<User> query = DbSet.Include(u => u.Profile);

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetPaged(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .Include(u => u.Profile)
            .OrderBy(u => u.RegistrationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}

public interface IUserRepository : IRepository<User>
{
    Task<IReadOnlyList<User>> GetPaged(int page, int pageSize, CancellationToken cancellationToken);
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken);
}
