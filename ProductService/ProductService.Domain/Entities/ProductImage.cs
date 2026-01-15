using System.Diagnostics.CodeAnalysis;

namespace ProductService.Domain.Entities;

[ExcludeFromCodeCoverage]
public class ProductImage : BaseEntity
{
    public required string Url { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product? Product { get; set; }
}
