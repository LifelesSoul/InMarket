using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Constants;
using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[ExcludeFromCodeCoverage]
public class User : BaseEntity
{
    [MaxLength(DbConstants.NameTextLength)]
    public required string Username { get; set; }

    [MaxLength(DbConstants.EmailTextLength)]
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRoles Role { get; set; }

    public DateTimeOffset RegistrationDate { get; set; } = TimeProvider.System.GetUtcNow();

    public virtual required UserProfile Profile { get; set; }

    public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();
}
