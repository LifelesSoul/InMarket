using ProductService.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.Models.Product;

[ExcludeFromCodeCoverage]
public class UpdateProductModel : BaseModel
{
    public required string Title { get; set; }

    public required decimal Price { get; set; }

    public string? Description { get; set; }

    public required ProductStatus Status { get; set; }

    public ICollection<string>? ImageUrls { get; set; }

    public required Guid CategoryId { get; set; }
}
