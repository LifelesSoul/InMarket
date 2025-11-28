namespace ProductService.Domain.Entities;

public class ProductImage
{
    public Guid Id { get; set; }

    public required string Url { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }
}
