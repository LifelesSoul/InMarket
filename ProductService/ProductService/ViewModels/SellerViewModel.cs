using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.ViewModels.User;

[ExcludeFromCodeCoverage]
public class SellerViewModel
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;
}
