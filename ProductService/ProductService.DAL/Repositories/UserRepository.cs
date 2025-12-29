using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using UserService.Domain.Entities;

namespace ProductService.DAL.Repositories;

public class UserRepository(ProductDbContext context) : Repository<User>(context), IUserRepository
{
    public override async Task<User?> GetById(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Profile)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetPaged(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(u => u.Profile)
            .OrderBy(u => u.RegistrationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
public interface IUserRepository : IRepository<User>
{
    Task<IReadOnlyList<User>> GetPaged(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);
}
