namespace ProductService.BLL.DTO.Product;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PriceString { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
