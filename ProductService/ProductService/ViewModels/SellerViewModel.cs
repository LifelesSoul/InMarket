using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.ViewModels.User;

[ExcludeFromCodeCoverage]
public class SellerViewModel
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTimeOffset RegistrationDate { get; set; }
}
