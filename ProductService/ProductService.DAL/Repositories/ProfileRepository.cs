using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using UserService.Domain.Entities;

namespace ProductService.DAL.Repositories;

public interface IProfileRepository
{
    Task<User?> GetUserWithProfileAsync(string externalId);
}

public class ProfileRepository : IProfileRepository
{
    private readonly ProductDbContext _context;

    public ProfileRepository(ProductDbContext context) => _context = context;

    public async Task<User?> GetUserWithProfileAsync(string externalId)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.ExternalId == externalId);
    }
}
