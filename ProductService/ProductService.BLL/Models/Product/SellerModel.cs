using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.Product;

[ExcludeFromCodeCoverage]
public class SellerModel : BaseModel
{
    public required string Username { get; set; }

    public required string Email { get; set; }
}
