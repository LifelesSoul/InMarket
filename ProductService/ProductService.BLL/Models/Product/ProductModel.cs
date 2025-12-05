using ProductService.Domain.Enums;

namespace ProductService.BLL.Models.Product;

public class ProductModel
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required decimal Price { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Priority Priority { get; set; }

    public ProductStatus Status { get; set; }

    public required CategoryModel Category { get; set; }

    public required SellerModel Seller { get; set; }

    public List<string> ImageUrls { get; set; } = [];
}
