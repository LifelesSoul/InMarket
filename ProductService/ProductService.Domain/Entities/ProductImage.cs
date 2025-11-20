namespace ProductService.Domain.Entities;

public sealed class ProductImage
{
    public required Guid Id { get; set; }

    public required string Url { get; set; }

    public required Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
