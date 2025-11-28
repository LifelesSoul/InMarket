namespace ProductService.BLL.Models.Product;

public class CreateProductModel
{
    public required string Title { get; set; }
    public required decimal Price { get; set; }
    public string? Description { get; set; }

    public List<string>? ImageUrls { get; set; }

    public required Guid CategoryId { get; set; }

    public required Guid SellerId { get; set; }
}
