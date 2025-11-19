using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User
{
    public required Guid Id { get; set; }

    [MaxLength(100)]
    public required string Username { get; set; }

    [MaxLength(250)]
    [EmailAddress]
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public required virtual UserProfile Profile { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}