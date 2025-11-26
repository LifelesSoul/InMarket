namespace ProductService.BLL.DTO.Product;

public class CreateProductModel
{
    public required string Title { get; set; }
    public required decimal Price { get; set; }

    public required Guid CategoryId { get; set; }

    public required Guid SellerId { get; set; }
}
