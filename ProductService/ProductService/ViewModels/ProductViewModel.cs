using ProductService.API.ViewModels.User;
using ProductService.Domain.Enums;

namespace ProductService.BLL.Models.Product;

public class ProductViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; } = 0m;

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Priority Priority { get; set; } = Priority.Low;

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public string? ImageUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = TimeProvider.System.GetUtcNow();

    public SellerViewModel Seller { get; set; } = null!;
}
