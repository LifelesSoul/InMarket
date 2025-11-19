using ProductService.Domain.Constants;
using ProductService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Entities;

namespace ProductService.Domain.Entities;

public class Product
{
    public required Guid Id { get; set; }

    [MaxLength(DbConstants.TitleTextLength)]
    public required string Title { get; set; }
    public string? Description { get; set; }

    [Column(TypeName = DbConstants.MoneyType)]
    public required decimal Price { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public required Priority Priority { get; set; }

    [Column(TypeName = DbConstants.StatusColumnType)]
    public required ProductStatus Status { get; set; }

    public required Guid CategoryId { get; set; }
    public required virtual Category Category { get; set; }
    public required Guid SellerId { get; set; }
    public required virtual User Seller { get; set; }

    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}