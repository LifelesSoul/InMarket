using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Constants;
using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User
{
    public required Guid Id { get; set; }

    [MaxLength(DbConstants.NameTextLength)]
    public required string Username { get; set; }

    [MaxLength(DbConstants.EmailTextLength)]
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }

    public DateTimeOffset RegistrationDate { get; set; } = TimeProvider.System.GetUtcNow();

    public virtual required UserProfile Profile { get; set; }

    public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();
}
