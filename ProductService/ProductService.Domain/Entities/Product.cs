using ProductService.Domain.Constants;
using ProductService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Entities;

namespace ProductService.Domain.Entities;

public sealed class Product
{
    public required Guid Id { get; set; }

    [MaxLength(DbConstants.TitleTextLength)]
    public required string Title { get; set; }
    public string? Description { get; set; }

    [Column(TypeName = DbConstants.MoneyType)]
    public required decimal Price { get; set; }
    public DateTimeOffset CreationDate { get; set; } = TimeProvider.System.GetUtcNow();
    public required Priority Priority { get; set; }

    [Column(TypeName = DbConstants.Nvarhar50Type)]
    public required ProductStatus Status { get; set; }

    public required Guid CategoryId { get; set; }
    public required Category Category { get; set; }

    public required Guid SellerId { get; set; }
    public required User Seller { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
