using ProductService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class User
{
    public required Guid Id { get; set; }

    public required string Username { get; set; }
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public required virtual UserProfile Profile { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
}
