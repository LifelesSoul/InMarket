using ProductService.Domain.Enums;

namespace ProductService.BLL.Models.Product;

public class UpdateProductModel
{
    public required string Title { get; set; }
    public required decimal Price { get; set; }
    public string? Description { get; set; }
    public required ProductStatus Status { get; set; }

    public ICollection<string>? ImageUrls { get; set; }

    public required Guid CategoryId { get; set; }
}
