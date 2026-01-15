using System.Diagnostics.CodeAnalysis;
using UserService.Domain.Enums;

namespace ProductService.BLL.Models.User;

[ExcludeFromCodeCoverage]
public class CreateUserModel
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public UserRole Role { get; set; } = UserRoles.BuyerOnly;
}
