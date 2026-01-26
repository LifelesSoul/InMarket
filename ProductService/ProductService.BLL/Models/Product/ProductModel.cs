using ProductService.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.Product;

[ExcludeFromCodeCoverage]
public class ProductModel : BaseModel
{
    public required string Title { get; set; }

    public required decimal Price { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreationDate { get; set; }

    public Priority Priority { get; set; }

    public ProductStatus Status { get; set; }

    public required ProductCategoryModel Category { get; set; }

    public required SellerModel Seller { get; set; }

    public List<string> ImageUrls { get; set; } = [];
}
