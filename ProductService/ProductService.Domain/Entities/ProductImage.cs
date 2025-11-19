namespace ProductService.Domain.Entities;

public class ProductImage
{
    public required Guid Id { get; set; }

    public required string Url { get; set; }

    public required Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }
}